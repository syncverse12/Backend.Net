using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.Meeting.TaskExtraction;
using SyncVerse.Application.Interfaces.AI.Meeting.TaskExtraction;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

namespace SyncVerse.Application.Services.AI
{
    public class AiTaskExtractionService : IAiTaskExtractionService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string SecretKey = "SyncVerse_Super_Secret_Key_For_Audio_Verification_2026"; 

        public AiTaskExtractionService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Result<AiTaskExtractionResponseDto>> ExtractTasksAsync(AiTaskExtractionRequestDto dto)
        {
            // ❌ الـ Restriction الصارم: التحقق من التوقيع الرقمي لمنع فبركة النص!
            if (!VerifySignature(dto.MeetingId.ToString(), dto.Transcript, dto.Signature))
            {
                return Result<AiTaskExtractionResponseDto>.Failure("Security Restriction Violation: Transcript has been altered or is not from the recorded audio!");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("AI_Task_Extraction_Server");
                var jsonPayload = JsonSerializer.Serialize(dto);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("extract-tasks", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseStream = await response.Content.ReadAsStreamAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = await JsonSerializer.DeserializeAsync<AiTaskExtractionResponseDto>(responseStream, options);

                    if (result == null)
                        return Result<AiTaskExtractionResponseDto>.Failure("Failed to deserialize AI Response.");

                    return Result<AiTaskExtractionResponseDto>.Success(result, "Tasks extracted successfully.");
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                return Result<AiTaskExtractionResponseDto>.Failure($"AI Task Extraction Server error ({response.StatusCode}): {errorBody}");
            }
            catch (Exception ex)
            {
                return Result<AiTaskExtractionResponseDto>.Failure($"Failed to communicate with AI Task Extraction Server: {ex.Message}");
            }
        }

        // 🔒 الدالة المساعدة للتحقق من التوقيع
        private bool VerifySignature(string meetingId, string text, string providedSignature)
        {
            var rawData = $"{meetingId}:{text}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            var calculatedSignature = Convert.ToBase64String(hashBytes);
            return calculatedSignature == providedSignature;
        }
    }
}