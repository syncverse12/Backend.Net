using System.Net.Http.Json;
using System.Text.Json;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.TaskAssignment;
using SyncVerse.Application.Interfaces.AI.TaskAssignment;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SyncVerse.Domain.Entities;
using System.Collections.Generic;
namespace SyncVerse.Application.Services.AI.TaskAssignment
{
    public class AiTaskAssignmentService : IAiTaskAssignmentService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SyncVerse.Application.Interfaces.Persistence.IUnitOfWork _unitOfWork;

        public AiTaskAssignmentService(IHttpClientFactory httpClientFactory, SyncVerse.Application.Interfaces.Persistence.IUnitOfWork unitOfWork)
        {
            _httpClientFactory = httpClientFactory;
            _unitOfWork = unitOfWork;
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

        public async Task<Result<object>> AddEmployeeAsync(AiAddProjectEmployeesRequestDto requestDto)
        {
            try
            {
                var projectMembersRepo = _unitOfWork.Repository<ProjectMember>();
                var taskRepo = _unitOfWork.Repository<global::TaskItem>();
                
                var members = await projectMembersRepo.Query()
                    .Include(pm => pm.User)
                    .Where(pm => pm.ProjectId == requestDto.ProjectId)
                    .ToListAsync();

                var client = _httpClientFactory.CreateClient("AI_Smart_Task_Assignment");
                var results = new List<object>();

                foreach (var pm in members)
                {
                    var user = pm.User;
                    if (user == null) continue;

                    var track = user.Skills != null && user.Skills.Any() ? user.Skills.First() : "Unknown";
                    var skills = user.Skills != null && user.Skills.Count > 1 ? user.Skills.Skip(1).ToList() : new List<string>();

                    var level = (int)user.SeniorityLevel > 4 ? "Senior" : "Junior";

                    var userTasks = await taskRepo.Query().Where(t => t.AssignedToUserId == user.Id).ToListAsync();
                    var activeTasks = userTasks.Count(t => t.Status == global::TaskStatus.Pending || t.Status == global::TaskStatus.InProgress);

                    var availabilityScore = 100.0;
                    try
                    {
                        availabilityScore = ((10.0 - activeTasks) / 10.0) * 100.0;
                    }
                    catch
                    {
                        availabilityScore = new Random().Next(50, 100);
                    }

                    var totalAssigned = userTasks.Count;
                    var completedTasks = userTasks.Count(t => t.Status == global::TaskStatus.Completed);
                    var pastSuccessRate = 0.85;
                    if (totalAssigned > 0)
                    {
                        pastSuccessRate = (double)completedTasks / totalAssigned;
                    }
                    else
                    {
                        pastSuccessRate = new Random().NextDouble() * (0.99 - 0.50) + 0.50;
                    }

                    var aiDto = new AiAddEmployeeRequestDto
                    {
                        Name = $"{user.FirstName} {user.LastName}",
                        Track = track,
                        Skills = skills,
                        Level = level,
                        Active_tasks = activeTasks,
                        Availability_score = (int)Math.Max(0, Math.Min(100, availabilityScore)),
                        Past_success_rate = Math.Round(pastSuccessRate, 2)
                    };

                    var response = await client.PostAsJsonAsync("add-employee", aiDto);

                    if (!response.IsSuccessStatusCode)
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        results.Add(new { User = aiDto.Name, Error = error, StatusCode = response.StatusCode });
                    }
                    else
                    {
                        var contentString = await response.Content.ReadAsStringAsync();
                        try 
                        {
                            var jsonObject = JsonSerializer.Deserialize<object>(contentString);
                            results.Add(new { User = aiDto.Name, Result = jsonObject ?? contentString });
                        }
                        catch
                        {
                            results.Add(new { User = aiDto.Name, Result = contentString });
                        }
                    }
                }

                return Result<object>.Success(results);
            }
            catch (Exception ex)
            {
                return Result<object>.Failure($"Failed to add employees: {ex.Message}");
            }
        }

        public async Task<Result<object>> UpdateEmployeeStatusAsync(AiUpdateEmployeeStatusFrontendRequestDto requestDto)
        {
            try
            {
                var userRepo = _unitOfWork.Repository<global::SyncVerse.Domain.Entities.User>();
                var taskRepo = _unitOfWork.Repository<global::TaskItem>();

                var user = await userRepo.Query().FirstOrDefaultAsync(u => u.Id == requestDto.UserId);
                if (user == null)
                {
                    return Result<object>.Failure("User not found.");
                }

                var userTasks = await taskRepo.Query().Where(t => t.AssignedToUserId == user.Id).ToListAsync();
                var currentActiveTasks = userTasks.Count(t => t.Status == global::TaskStatus.Pending || t.Status == global::TaskStatus.InProgress);
                
                int dbCompletedTasks = userTasks.Count(t => t.Status == global::TaskStatus.Completed);
                int dbTotalAssignedTasks = userTasks.Count;

                int delta = requestDto.ActiveTasks - currentActiveTasks;

                int calculatedTotalAssigned = dbTotalAssignedTasks;
                int calculatedCompleted = dbCompletedTasks;

                if (delta > 0)
                {
                    // Active tasks increased -> implies they got new tasks assigned
                    calculatedTotalAssigned += delta;
                }
                else if (delta < 0)
                {
                    // Active tasks reduced -> implies they completed some tasks
                    calculatedCompleted += Math.Abs(delta);
                }

                double pastSuccessRate = 0.85;
                if (calculatedTotalAssigned > 0)
                {
                    pastSuccessRate = (double)calculatedCompleted / calculatedTotalAssigned;
                }
                else
                {
                    pastSuccessRate = new Random().NextDouble() * (0.99 - 0.50) + 0.50;
                }

                int availabilityScore = 100;
                try
                {
                    availabilityScore = (int)(((10.0 - requestDto.ActiveTasks) / 10.0) * 100.0);
                    availabilityScore = Math.Max(0, Math.Min(100, availabilityScore));
                }
                catch
                {
                    availabilityScore = new Random().Next(50, 100);
                }

                var client = _httpClientFactory.CreateClient("AI_Smart_Task_Assignment");
                
                var employeesResponse = await client.GetAsync("employees");
                if (!employeesResponse.IsSuccessStatusCode)
                {
                    return Result<object>.Failure($"Failed to fetch employees from AI server: {employeesResponse.StatusCode}");
                }
                
                var employeesJson = await employeesResponse.Content.ReadAsStringAsync();
                int employeeId = -1;
                string fullName = $"{user.FirstName} {user.LastName}";
                
                try 
                {
                    using var doc = JsonDocument.Parse(employeesJson);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var el in doc.RootElement.EnumerateArray())
                        {
                            if (el.TryGetProperty("name", out var nameProp) && nameProp.GetString() == fullName)
                            {
                                if (el.TryGetProperty("employee_id", out var idProp) && idProp.TryGetInt32(out var parsedId))
                                {
                                    employeeId = parsedId;
                                    break;
                                }
                                else if (el.TryGetProperty("id", out var idProp2) && idProp2.TryGetInt32(out var parsedId2))
                                {
                                    employeeId = parsedId2;
                                    break;
                                }
                            }
                        }
                    }
                    else if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("employees", out var empArray) && empArray.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var el in empArray.EnumerateArray())
                        {
                            if (el.TryGetProperty("name", out var nameProp) && nameProp.GetString() == fullName)
                            {
                                if (el.TryGetProperty("employee_id", out var idProp) && idProp.TryGetInt32(out var parsedId))
                                {
                                    employeeId = parsedId;
                                    break;
                                }
                                else if (el.TryGetProperty("id", out var idProp2) && idProp2.TryGetInt32(out var parsedId2))
                                {
                                    employeeId = parsedId2;
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Result<object>.Failure($"Failed to parse employees from AI server: {ex.Message}");
                }

                if (employeeId == -1)
                {
                    return Result<object>.Failure($"Employee with name '{fullName}' not found on AI server.");
                }

                var aiDto = new AiUpdateEmployeeStatusRequestDto
                {
                    Employee_id = employeeId,
                    Active_tasks = requestDto.ActiveTasks,
                    Availability_score = availabilityScore,
                    Past_success_rate = Math.Round(pastSuccessRate, 2)
                };
                
                var response = await client.PostAsJsonAsync("update-employee-status", aiDto);

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

        public async Task<Result<(int ActiveTasks, int AvailabilityScore)>> CalculateAvailabilityAsync(string userId)
        {
            try
            {
                var taskRepo = _unitOfWork.Repository<global::TaskItem>();
                
                var activeTasks = taskRepo.Query().Count(t => 
                    t.AssignedToUserId == userId && 
                    (t.Status == global::TaskStatus.Pending || 
                     t.Status == global::TaskStatus.InProgress));

                // 1. Set Max Tasks
                int maxTasks = 5;

                // 2. Calculate Availability
                int availabilityScore = 0;
                if (activeTasks < maxTasks)
                {
                    availabilityScore = (int)(((double)(maxTasks - activeTasks) / maxTasks) * 100);
                }

                return Result<(int ActiveTasks, int AvailabilityScore)>.Success((activeTasks, availabilityScore));
            }
            catch (Exception ex)
            {
                return Result<(int ActiveTasks, int AvailabilityScore)>.Failure($"Failed to calculate availability: {ex.Message}");
            }
        }
    }
}
