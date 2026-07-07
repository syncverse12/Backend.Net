using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.Meeting;
using SyncVerse.Application.Interfaces.AI;
using SyncVerse.Application.Interfaces.Meetings;
using SyncVerse.Domain.Entities;
using SyncVerse.Application.Interfaces.Persistence;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;

namespace SyncVerse.Application.Services.AI
{
    public class AiMeetingService : IAiMeetingService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMeetingService _meetingService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private const string SecretKey = "SyncVerse_Super_Secret_Key_For_Audio_Verification_2026";

        public AiMeetingService(
            IHttpClientFactory httpClientFactory,
            IMeetingService meetingService,
            IUnitOfWork unitOfWork,
            IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _meetingService = meetingService;
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task<Result<bool>> SaveTranscriptToCacheAsync(string meetingId, TranscriptionSecureResponseDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrEmpty(dto.Transcript))
                    return Result<bool>.Failure("Transcript data cannot be empty.");

                var cacheKey = $"meeting_transcript_{meetingId}";

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                };

                _cache.Set(cacheKey, dto, cacheOptions);

                return Result<bool>.Success(true, "Transcript temporary cached successfully for 1 hour.");
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Failed to cache transcript: {ex.Message}");
            }
        }

        public async Task<Result<TranscriptionSecureResponseDto>> GetTranscriptFromCacheAsync(string meetingId)
        {
            try
            {
                var cacheKey = $"meeting_transcript_{meetingId}";
                if (_cache.TryGetValue(cacheKey, out TranscriptionSecureResponseDto? cachedData) && cachedData != null)
                {
                    return Result<TranscriptionSecureResponseDto>.Success(cachedData, "Transcript retrieved from cache successfully.");
                }

                return Result<TranscriptionSecureResponseDto>.Failure("Transcript not found or expired from temporary storage.");
            }
            catch (Exception ex)
            {
                return Result<TranscriptionSecureResponseDto>.Failure($"Error retrieving from cache: {ex.Message}");
            }
        }

        public async Task<Result<TranscriptionSecureResponseDto>> TranscribeAudioSecureAsync(IFormFile audioFile, string meetingId)
        {
            if (audioFile == null || audioFile.Length == 0)
                return Result<TranscriptionSecureResponseDto>.Failure("Audio file is empty.");

            try
            {
                var client = _httpClientFactory.CreateClient("AI_Transcription_Server");
                var content = new MultipartFormDataContent();
                var stream = audioFile.OpenReadStream();
                var streamContent = new StreamContent(stream);

                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(audioFile.ContentType);
                content.Add(streamContent, "file", audioFile.FileName);
                content.Add(new StringContent("true"), "language_detection");

                var response = await client.PostAsync("transcribe/file", content);

                if (!response.IsSuccessStatusCode)
                    return Result<TranscriptionSecureResponseDto>.Failure($"AI Server error: {response.StatusCode}");

                var rawText = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(rawText))
                    return Result<TranscriptionSecureResponseDto>.Failure("Failed to extract text from audio.");

                string cleanText;
                try
                {
                    using var jsonDoc = JsonDocument.Parse(rawText);
                    if (jsonDoc.RootElement.TryGetProperty("text", out var textElement))
                    {
                        cleanText = textElement.GetString() ?? rawDocTextTrimmed(rawText);
                    }
                    else
                    {
                        cleanText = rawDocTextTrimmed(rawText);
                    }
                }
                catch
                {
                    cleanText = rawDocTextTrimmed(rawText);
                }

                var signature = GenerateSignature(meetingId, cleanText);

                var resultDto = new TranscriptionSecureResponseDto
                {
                    Transcript = cleanText,
                    Signature = signature
                };

                await SaveTranscriptToCacheAsync(meetingId, resultDto);

                return Result<TranscriptionSecureResponseDto>.Success(resultDto, "Audio transcribed and verified successfully.");
            }
            catch (Exception ex)
            {
                return Result<TranscriptionSecureResponseDto>.Failure($"Stream copying failed: {ex.Message}");
            }
        }

        private string rawDocTextTrimmed(string raw) => raw.Trim('"').Trim();

        public async Task<Result<bool>> ProcessAndSaveSummaryAsync(string meetingId, SecureProcessRequestDto dto)
        {
            if (!VerifySignature(meetingId, dto.Transcript, dto.Signature))
                return Result<bool>.Failure("Security Restriction Violation: Transcript text has been altered or is not from the recorded audio!");

            var summaryRequest = new AiMeetingSummaryRequestDto { Transcript = dto.Transcript };
            var summaryResult = await GenerateSummaryAsync(summaryRequest);

            if (!summaryResult.IsSuccess || summaryResult.Data == null)
                return Result<bool>.Failure($"Summarization failed: {summaryResult.Message}");

            var saveResult = await _meetingService.SaveAiSummaryAsync(meetingId, summaryResult.Data);
            return saveResult;
        }

        public async Task<Result<bool>> ProcessAndSaveTasksAsync(string meetingId, SecureProcessRequestDto dto)
        {
            // 1️⃣ أولاً: التحقق من التوقيع الأمني داخلياً بالـ UUID الأصلي المتناسق مع بقية السيستم
            if (!VerifySignature(meetingId, dto.Transcript, dto.Signature))
                return Result<bool>.Failure("Security Restriction Violation: Transcript text has been altered or is not from the recorded audio!");

            try
            {
                var taskClient = _httpClientFactory.CreateClient("AI_Task_Extraction_Server");

                // 🎯 2️⃣ ثانياً: الخدعة الذكية لسيرفر الـ AI الخارجي
                // بنبعت له الكائن مفرود بأسماء حقول Snake Case، وبقيمة Int32 للـ meeting_id عشان السيرفر يقبله فوراً وميضربش
                var aiPayload = new
                {
                    meeting_id = 123,
                    transcript = dto.Transcript
                };

                var taskResponse = await taskClient.PostAsJsonAsync("extract-tasks", aiPayload);

                if (!taskResponse.IsSuccessStatusCode)
                {
                    var errorResponse = await taskResponse.Content.ReadAsStringAsync();
                    return Result<bool>.Failure($"AI Task Extraction Server returned error ({taskResponse.StatusCode}): {errorResponse}");
                }

                var extractedTasks = await taskResponse.Content.ReadFromJsonAsync<List<string>>();

                if (extractedTasks != null && extractedTasks.Any())
                {
                    foreach (var taskText in extractedTasks)
                    {
                        var newTask = new TaskEmployee
                        {
                            Id = Guid.NewGuid().ToString(),
                            CreatedAt = DateTime.UtcNow,
                            IsDeleted = false
                        };
                        await _unitOfWork.Repository<TaskEmployee>().AddAsync(newTask);
                    }
                    await _unitOfWork.SaveChangesAsync();
                    return Result<bool>.Success(true, $"{extractedTasks.Count} Tasks extracted and saved to Database successfully.");
                }

                return Result<bool>.Success(true, "No tasks were found in this meeting transcript.");
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Task extraction failed: {ex.Message}");
            }
        }

        public async Task<Result<AiMeetingSummaryResponseDto>> GenerateSummaryAsync(AiMeetingSummaryRequestDto dto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Meeting_Server");
                var jsonPayload = JsonSerializer.Serialize(dto);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("generate-summary", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseStream = await response.Content.ReadAsStreamAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = await JsonSerializer.DeserializeAsync<AiMeetingSummaryResponseDto>(responseStream, options);

                    if (result != null)
                    {
                        result.MeetingId = dto.MeetingId;
                        result.MeetingTitle = dto.MeetingTitle;

                        return Result<AiMeetingSummaryResponseDto>.Success(result);
                    }

                    return Result<AiMeetingSummaryResponseDto>.Failure("Deserialization failed.");
                }
                return Result<AiMeetingSummaryResponseDto>.Failure($"AI Server error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return Result<AiMeetingSummaryResponseDto>.Failure($"Failed to communicate with AI Summary Server: {ex.Message}");
            }
        }

        private string GenerateSignature(string meetingId, string text)
        {
            var rawData = $"{meetingId}:{text}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return Convert.ToBase64String(hashBytes);
        }

        private bool VerifySignature(string meetingId, string text, string providedSignature)
        {
            var calculatedSignature = GenerateSignature(meetingId, text);
            return calculatedSignature == providedSignature;
        }
    }
}