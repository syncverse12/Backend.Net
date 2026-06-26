using System.Text;
using System.Text.Json;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.Meeting;
using SyncVerse.Application.Interfaces.AI;

namespace SyncVerse.Application.Services.AI
{
    public class AiMeetingService : IAiMeetingService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AiMeetingService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Result<AiMeetingSummaryResponseDto>> GenerateSummaryAsync(AiMeetingSummaryRequestDto dto)
        {
            try
            {
                // نداء الـ Client اللي سجلناه في الـ Program.cs للسيرفر الجديد
                var client = _httpClientFactory.CreateClient("AI_Meeting_Server");

                var jsonPayload = JsonSerializer.Serialize(dto);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("generate-summary", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseStream = await response.Content.ReadAsStreamAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = await JsonSerializer.DeserializeAsync<AiMeetingSummaryResponseDto>(responseStream, options);

                    if (result == null)
                    {
                        return Result<AiMeetingSummaryResponseDto>.Failure("Failed to deserialize AI response.");
                    }

                    return Result<AiMeetingSummaryResponseDto>.Success(result, "Meeting summary generated successfully.");
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                return Result<AiMeetingSummaryResponseDto>.Failure($"AI Server error ({response.StatusCode}): {errorBody}");
            }
            catch (Exception ex)
            {
                return Result<AiMeetingSummaryResponseDto>.Failure($"Failed to communicate with AI Summary Server: {ex.Message}");
            }
        }
    }
}