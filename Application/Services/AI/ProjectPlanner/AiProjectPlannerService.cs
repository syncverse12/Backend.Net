using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.ProjectPlanner;
using SyncVerse.Application.Interfaces.AI.ProjectPlanner;

namespace SyncVerse.Application.Services.AI.ProjectPlanner
{
    public class AiProjectPlannerService : IAiProjectPlannerService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AiProjectPlannerService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Result<object>> CreateProjectPlanAsync(AiProjectPlanRequestDto requestDto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Project_Planner_Server");
                
                var response = await client.PostAsJsonAsync("plan", requestDto);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Result<object>.Failure($"AI Project Planner Server returned error: {response.StatusCode}. Details: {error}");
                }

                var contentString = await response.Content.ReadAsStringAsync();
                
                try 
                {
                    var jsonObject = JsonSerializer.Deserialize<object>(contentString);
                    return Result<object>.Success(jsonObject ?? contentString);
                }
                catch
                {
                    return Result<object>.Success(contentString);
                }
            }
            catch (Exception ex)
            {
                return Result<object>.Failure($"Failed to generate project plan: {ex.Message}");
            }
        }

        public async Task<Result<object>> GetProjectPlanAsync(string projectId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Project_Planner_Server");
                
                var response = await client.GetAsync($"plan/{projectId}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return Result<object>.Failure($"Project plan not found for ID: {projectId}");
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    return Result<object>.Failure($"AI Project Planner Server returned error: {response.StatusCode}. Details: {error}");
                }

                var contentString = await response.Content.ReadAsStringAsync();
                
                try 
                {
                    var jsonObject = JsonSerializer.Deserialize<object>(contentString);
                    return Result<object>.Success(jsonObject ?? contentString);
                }
                catch
                {
                    return Result<object>.Success(contentString);
                }
            }
            catch (Exception ex)
            {
                return Result<object>.Failure($"Failed to retrieve project plan: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteProjectPlanAsync(string projectId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Project_Planner_Server");
                
                var response = await client.DeleteAsync($"plan/{projectId}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return Result<bool>.Failure($"Project plan not found for ID: {projectId}");
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    return Result<bool>.Failure($"AI Project Planner Server returned error: {response.StatusCode}. Details: {error}");
                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Failed to delete project plan: {ex.Message}");
            }
        }
    }
}
