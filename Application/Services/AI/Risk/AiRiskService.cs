using System.Text.Json;
using SyncVerse.Application.Interfaces.AI.Risk;

namespace SyncVerse.Application.Services.AI.Risk
{
    public class AiRiskService : IAiRiskService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AiRiskService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<object> AnalyzeProjectRisksAsync(Guid projectId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Risk_Server");
                var url = $"/projects/{projectId}/analyze";

                var response = await client.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    var result = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);
                    return result ?? new Dictionary<string, object>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new { error = $"AI Server returned {response.StatusCode}", details = errorContent };
                }
            }
            catch (Exception ex)
            {
                return new { error = "Exception caught in service", message = ex.Message };
            }
        }

        public async Task<object> GetProjectRiskHistoryAsync(Guid projectId, int limit = 20)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Risk_Server");
                var url = $"/projects/{projectId}/history?limit={limit}";

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var jsonResult = JsonSerializer.Deserialize<object>(content);
                        return jsonResult ?? content;
                    }
                    catch
                    {
                        return content; 
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new { error = $"AI Server returned {response.StatusCode}", details = errorContent };
                }
            }
            catch (Exception ex)
            {
                return new { error = "Exception caught in history service", message = ex.Message };
            }
        }
    }
}