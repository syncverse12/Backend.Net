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

namespace SyncVerse.Application.Services.AI
{
    public class AiMeetingService : IAiMeetingService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMeetingService _meetingService;
        private readonly IUnitOfWork _unitOfWork;
        private const string SecretKey = "SyncVerse_Super_Secret_Key_For_Audio_Verification_2026"; // مفتاح تشفير خاص بالسيرفر

        public AiMeetingService(
            IHttpClientFactory httpClientFactory,
            IMeetingService meetingService,
            IUnitOfWork unitOfWork)
        {
            _httpClientFactory = httpClientFactory;
            _meetingService = meetingService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<TranscriptionSecureResponseDto>> TranscribeAudioSecureAsync(IFormFile audioFile, string meetingId)
        {
            if (audioFile == null || audioFile.Length == 0)
                return Result<TranscriptionSecureResponseDto>.Failure("Audio file is empty.");

            var client = _httpClientFactory.CreateClient("AI_Transcription_Server");
            using var content = new MultipartFormDataContent();
            using var stream = audioFile.OpenReadStream();
            using var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(audioFile.ContentType);
            content.Add(streamContent, "file", audioFile.FileName);
            content.Add(new StringContent("true"), "language_detection");

            var response = await client.PostAsync("transcribe/file", content);
            if (!response.IsSuccessStatusCode)
                return Result<TranscriptionSecureResponseDto>.Failure($"AI Server error: {response.StatusCode}");

            var transcriptionResult = await response.Content.ReadFromJsonAsync<AiTranscriptionResponseDto>();
            if (transcriptionResult == null || string.IsNullOrEmpty(transcriptionResult.Text))
                return Result<TranscriptionSecureResponseDto>.Failure("Failed to extract text from audio.");

            var signature = GenerateSignature(meetingId, transcriptionResult.Text);

            return Result<TranscriptionSecureResponseDto>.Success(new TranscriptionSecureResponseDto
            {
                Transcript = transcriptionResult.Text,
                Signature = signature
            }, "Audio transcribed and verified successfully.");
        }

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
            if (!VerifySignature(meetingId, dto.Transcript, dto.Signature))
                return Result<bool>.Failure("Security Restriction Violation: Transcript text has been altered or is not from the recorded audio!");

            try
            {
                var taskClient = _httpClientFactory.CreateClient("AI_Task_Extraction_Server");
                var taskResponse = await taskClient.PostAsJsonAsync("extract-tasks", new { text = dto.Transcript });

                if (!taskResponse.IsSuccessStatusCode)
                    return Result<bool>.Failure("AI Task Extraction Server returned error.");

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
                    return result != null ? Result<AiMeetingSummaryResponseDto>.Success(result) : Result<AiMeetingSummaryResponseDto>.Failure("Deserialization failed.");
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