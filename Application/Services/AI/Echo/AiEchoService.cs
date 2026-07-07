using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.Echo;
using SyncVerse.Application.Interfaces.AI.Echo;
using System.Text.Json;

namespace SyncVerse.Application.Services.AI.Echo
{
    public class AiEchoService : IAiEchoService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public AiEchoService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };
        }

        public async Task<Result<EchoChatResponseDto>> TalkToEchoAsync(EchoChatRequestDto dto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Echo_Server");

                var response = await client.PostAsJsonAsync("/echo/chat", dto);

                if (response.IsSuccessStatusCode)
                {
                    var chatResult = await response.Content.ReadFromJsonAsync<EchoChatResponseDto>(_jsonOptions);
                    return chatResult != null
                        ? Result<EchoChatResponseDto>.Success(chatResult, "Echo responded successfully.")
                        : Result<EchoChatResponseDto>.Failure("Failed to deserialize Echo's response.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return Result<EchoChatResponseDto>.Failure($"Echo AI Error: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex)
            {
                return Result<EchoChatResponseDto>.Failure($"Communication with Echo failed: {ex.Message}");
            }
        }
    }
}