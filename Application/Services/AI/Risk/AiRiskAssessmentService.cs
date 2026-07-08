using System.Net.Http.Json;
using System.Text.Json;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.Risk;
using SyncVerse.Application.Interfaces.AI.Risk;

namespace SyncVerse.Application.Services.AI
{
    public class AiRiskAssessmentService : IAiRiskAssessmentService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AiRiskAssessmentService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Result<ProjectRiskAssessmentResponseDto>> AnalyzeProjectRisksAsync(string projectId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Risk_Server");

                var response = await client.PostAsync($"/projects/{projectId}/analyze", null);

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = await response.Content.ReadFromJsonAsync<ProjectRiskAssessmentResponseDto>(options);

                    return result != null
                        ? Result<ProjectRiskAssessmentResponseDto>.Success(result, "Project risks analyzed successfully.")
                        : Result<ProjectRiskAssessmentResponseDto>.Failure("Failed to deserialize risk assessment data.");
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                return Result<ProjectRiskAssessmentResponseDto>.Failure($"AI Risk Server error ({response.StatusCode}): {errorBody}");
            }
            catch (Exception ex)
            {
                return Result<ProjectRiskAssessmentResponseDto>.Failure($"Failed to communicate with AI Risk Server: {ex.Message}");
            }
        }
    }
}