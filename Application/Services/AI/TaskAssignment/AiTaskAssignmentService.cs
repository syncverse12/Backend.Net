using System.Net.Http.Json;
using System.Text.Json;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.TaskAssignment;
using SyncVerse.Application.Interfaces.AI.TaskAssignment;

namespace SyncVerse.Application.Services.AI.TaskAssignment
{
    public class AiTaskAssignmentService : IAiTaskAssignmentService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AiTaskAssignmentService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Result<AiTaskAnalysisResponseDto>> AnalyzeTaskAsync(AiTaskAnalysisRequestDto requestDto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Smart_Task_Assignment");
                
                var response = await client.PostAsJsonAsync("analyze-task", requestDto);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Result<AiTaskAnalysisResponseDto>.Failure($"AI Task Assignment Server returned error: {response.StatusCode}. Details: {error}");
                }

                // The endpoint returns a plain string containing the task_id
                var taskId = await response.Content.ReadAsStringAsync();

                // Clean the string if it's returned with quotes
                taskId = taskId?.Trim('\"', ' ', '\n', '\r');

                if (string.IsNullOrEmpty(taskId))
                {
                    return Result<AiTaskAnalysisResponseDto>.Failure("Failed to extract task_id from AI Server response.");
                }

                return Result<AiTaskAnalysisResponseDto>.Success(new AiTaskAnalysisResponseDto { TaskId = taskId });
            }
            catch (Exception ex)
            {
                return Result<AiTaskAnalysisResponseDto>.Failure($"Task analysis failed: {ex.Message}");
            }
        }
        public async Task<Result<object>> AnalyzeTaskSyncAsync(AiTaskAnalysisRequestDto requestDto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Smart_Task_Assignment");
                
                var response = await client.PostAsJsonAsync("analyze-task/sync", requestDto);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Result<object>.Failure($"AI Task Assignment Server returned error: {response.StatusCode}. Details: {error}");
                }

                var contentString = await response.Content.ReadAsStringAsync();
                
                try 
                {
                    var jsonObject = JsonSerializer.Deserialize<object>(contentString);
                    return Result<object>.Success(jsonObject ?? contentString);
                }
                catch
                {
                    // If it's not JSON, just return the string
                    return Result<object>.Success(contentString);
                }
            }
            catch (Exception ex)
            {
                return Result<object>.Failure($"Synchronous task analysis failed: {ex.Message}");
            }
        }

        public async Task<Result<object>> GetEmployeesAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Smart_Task_Assignment");
                
                var response = await client.GetAsync("employees");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Result<object>.Failure($"AI Task Assignment Server returned error: {response.StatusCode}. Details: {error}");
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
                return Result<object>.Failure($"Failed to fetch employees: {ex.Message}");
            }
        }

        public async Task<Result<object>> AddEmployeeAsync(AiAddEmployeeRequestDto requestDto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Smart_Task_Assignment");
                
                var response = await client.PostAsJsonAsync("add-employee", requestDto);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Result<object>.Failure($"AI Task Assignment Server returned error: {response.StatusCode}. Details: {error}");
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
                return Result<object>.Failure($"Failed to add employee: {ex.Message}");
            }
        }

        public async Task<Result<object>> UpdateEmployeeStatusAsync(AiUpdateEmployeeStatusRequestDto requestDto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Smart_Task_Assignment");
                
                var response = await client.PostAsJsonAsync("update-employee-status", requestDto);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Result<object>.Failure($"AI Task Assignment Server returned error: {response.StatusCode}. Details: {error}");
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
                return Result<object>.Failure($"Failed to update employee status: {ex.Message}");
            }
        }

        public async Task<Result<object>> CheckRootAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Smart_Task_Assignment");
                var response = await client.GetAsync("");
                if (!response.IsSuccessStatusCode)
                    return Result<object>.Failure($"AI Server Root returned error: {response.StatusCode}");

                var contentString = await response.Content.ReadAsStringAsync();
                try { return Result<object>.Success(JsonSerializer.Deserialize<object>(contentString) ?? contentString); }
                catch { return Result<object>.Success(contentString); }
            }
            catch (Exception ex) { return Result<object>.Failure($"Failed to reach AI Server Root: {ex.Message}"); }
        }

        public async Task<Result<object>> CheckHealthAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Smart_Task_Assignment");
                var response = await client.GetAsync("health");
                if (!response.IsSuccessStatusCode)
                    return Result<object>.Failure($"AI Server Health returned error: {response.StatusCode}");

                var contentString = await response.Content.ReadAsStringAsync();
                try { return Result<object>.Success(JsonSerializer.Deserialize<object>(contentString) ?? contentString); }
                catch { return Result<object>.Success(contentString); }
            }
            catch (Exception ex) { return Result<object>.Failure($"Failed to reach AI Server Health: {ex.Message}"); }
        }
    }
}
