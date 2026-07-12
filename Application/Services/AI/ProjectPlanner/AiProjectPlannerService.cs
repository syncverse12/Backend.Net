using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.ProjectPlanner;
using SyncVerse.Application.Interfaces.AI.ProjectPlanner;
using SyncVerse.Application.Interfaces.Persistence;
using SyncVerse.Domain.Entities;
using System.Linq;
using System.Collections.Generic;

namespace SyncVerse.Application.Services.AI.ProjectPlanner
{
    public class AiProjectPlannerService : IAiProjectPlannerService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUnitOfWork _unitOfWork;

        public AiProjectPlannerService(IHttpClientFactory httpClientFactory, IUnitOfWork unitOfWork)
        {
            _httpClientFactory = httpClientFactory;
            _unitOfWork = unitOfWork;
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

        public async Task<Result<object>> GetProjectPlanSummaryAsync(string projectId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Project_Planner_Server");
                
                var response = await client.GetAsync($"plan/{projectId}/summary");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return Result<object>.Failure($"Project plan summary not found for ID: {projectId}");
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
                return Result<object>.Failure($"Failed to retrieve project plan summary: {ex.Message}");
            }
        }

        public async Task<Result<object>> ReplanProjectAsync(string projectId, AiReplanRequestDto requestDto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Project_Planner_Server");
                
                var response = await client.PostAsJsonAsync($"plan/{projectId}/replan", requestDto);

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
                return Result<object>.Failure($"Failed to replan project: {ex.Message}");
            }
        }

        public async Task<Result<object>> GetAllProjectPlansAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Project_Planner_Server");
                
                var response = await client.GetAsync("plans");

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
                return Result<object>.Failure($"Failed to retrieve all project plans: {ex.Message}");
            }
        }

        public async Task<Result<object>> CheckHealthAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AI_Project_Planner_Server");
                
                var response = await client.GetAsync("health");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Result<object>.Failure($"AI Project Planner Server health check returned error: {response.StatusCode}. Details: {error}");
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
                return Result<object>.Failure($"Failed to reach AI Project Planner Server: {ex.Message}");
            }
        }

        public async Task<Result<object>> GenerateScheduleForProjectAsync(string projectId)
        {
            try
            {
                var project = await _unitOfWork.Repository<SyncVerse.Domain.Entities.Project>().GetByIdAsync(projectId);
                if (project == null)
                {
                    return Result<object>.Failure($"Project not found for ID: {projectId}");
                }

                var allTasks = _unitOfWork.Repository<TaskItem>().Query().Where(t => t.ProjectId == projectId).ToList();
                var dependencies = _unitOfWork.Repository<TaskDependency>().Query().Where(d => allTasks.Select(t => t.Id).Contains(d.TaskId) || allTasks.Select(t => t.Id).Contains(d.DependsOnTaskId)).ToList();
                var projectMembers = _unitOfWork.Repository<ProjectMember>().Query().Where(m => m.ProjectId == projectId).ToList(); // Ideally Include(User)
                
                // Fallback since we might not have Include readily available here without EF Core reference
                var allUsers = _unitOfWork.Repository<User>().Query().ToList();

                var aiTasks = allTasks.Select(t => new AiPlannerTaskDto
                {
                    Id = t.Id,
                    Name = t.Title,
                    Description = t.Description ?? "",
                    Estimated_hours = t.Priority switch
                    {
                        SyncVerse.Domain.Enums.TaskPriority.Low => 4,
                        SyncVerse.Domain.Enums.TaskPriority.Medium => 8,
                        SyncVerse.Domain.Enums.TaskPriority.High => 16,
                        SyncVerse.Domain.Enums.TaskPriority.Critical => 24,
                        _ => 8
                    },
                    Dependencies = dependencies.Where(d => d.TaskId == t.Id).Select(d => d.DependsOnTaskId).ToList(),
                    Priority = t.Priority.ToString().ToLower(), // Fix: AI expects lowercase
                    Required_skills = new List<string> { "general" }, // Static as requested
                    Is_milestone = false, // Static as requested
                    Metadata = new Dictionary<string, object>() // Fix: AI expects a valid dictionary, not null
                }).ToList();

                var aiResources = projectMembers.Select(m => {
                    var user = allUsers.FirstOrDefault(u => u.Id == m.UserId);
                    var skills = user?.Skills?.ToList() ?? new List<string> { "general" };
                    if (!skills.Contains("general")) skills.Add("general"); // Ensure AI can match the tasks

                    return new AiPlannerResourceDto
                    {
                        Id = m.UserId,
                        Name = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
                        Capacity = 1.0f, // Static full capacity
                        Skills = skills,
                        Available_from = project.StartDate.ToString("yyyy-MM-dd"),
                        Available_until = project.EndDate.ToString("yyyy-MM-dd")
                    };
                }).ToList();

                var requestDto = new AiProjectPlanRequestDto
                {
                    Project_name = project.Name,
                    Deadline = project.EndDate.ToString("yyyy-MM-dd"),
                    Project_start = project.StartDate.ToString("yyyy-MM-dd"),
                    Sprint_length_days = 14, // Static default
                    Hours_per_day = 8, // Static default
                    Tasks = aiTasks,
                    Resources = aiResources
                };

                // Call existing Create method
                return await CreateProjectPlanAsync(requestDto);
            }
            catch (Exception ex)
            {
                return Result<object>.Failure($"Failed to generate schedule for project: {ex.Message}");
            }
        }
    }
}
