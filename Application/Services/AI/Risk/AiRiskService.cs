using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.Risk;
using SyncVerse.Application.Interfaces.AI.Risk;
using System.Text.Json;


namespace SyncVerse.Application.Services.AI
{
    public class AiRiskService : IAiRiskService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AiRiskService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Result<ProjectRiskResponseDto>> AnalyzeProjectRisksAsync(ProjectRiskRequestDto dto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Risk_Server");

                var response = await client.PostAsJsonAsync("api/v1/risk/analyze-project", dto);

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = await response.Content.ReadFromJsonAsync<ProjectRiskResponseDto>(options);

                    return result != null
                        ? Result<ProjectRiskResponseDto>.Success(result, "Project risk analysis generated successfully.")
                        : Result<ProjectRiskResponseDto>.Failure("Failed to deserialize risk report.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return Result<ProjectRiskResponseDto>.Failure($"Risk Engine Error: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex)
            {
                return Result<ProjectRiskResponseDto>.Failure($"Failed to communicate with Risk Engine: {ex.Message}");
            }
        }

        public async Task<Result<ProjectRiskResponseDto>> UpdateLiveRisksAsync(LiveRiskUpdateRequestDto dto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Risk_Server");

                var response = await client.PostAsJsonAsync("api/v1/risk/live-update", dto);

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = await response.Content.ReadFromJsonAsync<ProjectRiskResponseDto>(options);

                    return result != null
                        ? Result<ProjectRiskResponseDto>.Success(result, "Live project risk metrics updated successfully.")
                        : Result<ProjectRiskResponseDto>.Failure("Failed to deserialize live risk report.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return Result<ProjectRiskResponseDto>.Failure($"Risk Engine Live Error: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex)
            {
                return Result<ProjectRiskResponseDto>.Failure($"Failed to communicate with Risk Engine on Live Update: {ex.Message}");
            }
        }
    }
}