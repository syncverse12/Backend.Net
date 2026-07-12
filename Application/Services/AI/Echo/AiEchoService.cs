using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.Echo;
using SyncVerse.Application.Interfaces.AI.Echo;  

namespace SyncVerse.Application.Services.AI.Echo
{
    public class AiEchoService : IAiEchoService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AiEchoService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Result<EchoChatResponseDto>> TalkToEchoAsync(EchoChatRequestDto dto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Echo_Server");
                var response = await client.PostAsJsonAsync("echo/chat", dto);

                if (response.IsSuccessStatusCode)
                {
                    var chatResult = await response.Content.ReadFromJsonAsync<EchoChatResponseDto>();
                    return chatResult != null
                        ? Result<EchoChatResponseDto>.Success(chatResult, "Echo responded successfully.")
                        : Result<EchoChatResponseDto>.Failure("Failed to deserialize Echo's response.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return Result<EchoChatResponseDto>.Failure($"Echo AI Server error ({response.StatusCode}): {errorContent}");
            }
            catch (Exception ex)
            {
                return Result<EchoChatResponseDto>.Failure($"Communication with Echo failed: {ex.Message}");
            }
        }

        public async System.Threading.Tasks.Task SaveProjectMemoryAutomatedAsync(EchoMemoryUploadDto memoryDto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Echo_Server");

                var response = await client.PostAsJsonAsync("echo/memory", memoryDto);

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AiEchoService Error] SaveProjectMemoryAutomatedAsync failed! Reason: {ex.Message}");
                throw;
            }
        }

        public async Task<EchoTimelineResponseDto> GetProjectTimelineAsync(Guid projectId, int limit = 100, int offset = 0, string? memoryType = null, string? teamName = null)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Echo_Server");
                var url = $"echo/project/{projectId}/timeline?limit={limit}&offset={offset}";

                if (!string.IsNullOrEmpty(memoryType)) url += $"&memory_type={memoryType}";
                if (!string.IsNullOrEmpty(teamName)) url += $"&team_name={teamName}";

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<EchoTimelineResponseDto>();
                    return result ?? new EchoTimelineResponseDto { ProjectId = projectId.ToString() };
                }

                var err = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[AiEchoService Error] GetProjectTimelineAsync status: {response.StatusCode}, Content: {err}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AiEchoService Exception] Timeline communication failed: {ex.Message}");
            }

            return new EchoTimelineResponseDto { ProjectId = projectId.ToString(), Items = new() };
        }

        public async Task<EchoWeeklySummaryResponseDto> GetWeeklySummaryAsync(Guid projectId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Echo_Server");

                var url = $"echo/summary/week?project_id={projectId}";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<EchoWeeklySummaryResponseDto>();
                    return result ?? new EchoWeeklySummaryResponseDto { ProjectId = projectId.ToString() };
                }

                var err = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[AiEchoService Error] GetWeeklySummaryAsync status: {response.StatusCode}, Content: {err}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AiEchoService Exception] Weekly Summary communication failed: {ex.Message}");
            }

            return new EchoWeeklySummaryResponseDto { ProjectId = projectId.ToString(), HighlightedMemories = new() };
        }
    }
}