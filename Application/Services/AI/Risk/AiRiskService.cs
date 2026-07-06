using Microsoft.EntityFrameworkCore;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.Risk;
using SyncVerse.Application.Interfaces.AI.Risk;
using SyncVerse.Infrastructure.Data;
using System.Text.Json;

namespace SyncVerse.Application.Services.AI
{
    public class AiRiskService : IAiRiskService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DatabaseDbContext _context;
        private readonly JsonSerializerOptions _jsonOptions;

        public AiRiskService(IHttpClientFactory httpClientFactory, DatabaseDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };
        }

        public async Task<Result<bool>> SaveProjectRiskProfileAsync(Guid projectId, ProjectRiskProfileEnrichmentDto enrichmentData)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Workspace)
                    .Include(p => p.TeamMembers)
                        .ThenInclude(m => m.User)
                    .FirstOrDefaultAsync(p => p.Id == projectId.ToString());

                if (project == null)
                    return Result<bool>.Failure("Project not found in database.");

                var allTeamSkills = project.TeamMembers
                    .Where(m => m.User != null && m.User.Skills != null)
                    .SelectMany(m => m.User.Skills)
                    .Distinct()
                    .ToList();

                var teamLanguages = allTeamSkills
                    .Where(s => s.Equals("C#", StringComparison.OrdinalIgnoreCase) ||
                                s.Equals("Python", StringComparison.OrdinalIgnoreCase) ||
                                s.Equals("Dart", StringComparison.OrdinalIgnoreCase) ||
                                s.Equals("JavaScript", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var autoTeamList = project.TeamMembers.Select(m => {
                    int experienceYears = m.User?.SeniorityLevel switch
                    {
                        Domain.Enums.SeniorityLevel.Junior => 2,
                        Domain.Enums.SeniorityLevel.Senior => 5,
                        Domain.Enums.SeniorityLevel.Lead => 7,
                        Domain.Enums.SeniorityLevel.Fresh => 1,
                        Domain.Enums.SeniorityLevel.Intern => 0,
                        _ => 2
                    };

                    return new
                    {
                        name = m.User != null ? $"{m.User.FirstName} {m.User.LastName}" : "Unknown Member",
                        role = m.Role.ToString(),
                        skills = m.User?.Skills ?? new List<string>(),
                        current_workload_pct = 0.0,
                        seniority_years = experienceYears
                    };
                }).ToList();

                var aiPayload = new
                {
                    project_name = project.Name,
                    description = project.Description ?? string.Empty,
                    client_name = project.Workspace?.Name ?? "SyncVerse Client",
                    start_date = project.StartDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    deadline = project.EndDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    budget_usd = project.Budget.HasValue ? (double)project.Budget.Value : 0.0,

                    estimated_hours = enrichmentData.EstimatedHours,
                    client_responsiveness = enrichmentData.ClientResponsiveness,
                    third_party_integrations_count = enrichmentData.ThirdPartyIntegrationsCount,
                    similar_past_projects = enrichmentData.SimilarPastProjects,

                    team = autoTeamList,
                    tech_stack = new
                    {
                        languages = teamLanguages.Any() ? teamLanguages : new List<string> { "C#" },
                        frameworks = new List<string> { ".NET Core" }, 
                        infrastructure = string.IsNullOrEmpty(project.RepositoryUrl) ? new List<string>() : new List<string> { project.RepositoryUrl },
                        third_party_apis = new List<string>()
                    },
                    required_skills = allTeamSkills.Take(5).ToList(),

                    has_clear_requirements = true,
                    requirement_completeness_pct = 0.0,
                    dependencies_count = 0,
                    infrastructure_ready = true
                };

                var client = _httpClientFactory.CreateClient("AI_Risk_Server");
                var response = await client.PutAsJsonAsync($"/api/v1/risk/project-profile/{projectId}", aiPayload, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    return Result<bool>.Success(true, "Project profile synchronized dynamically and saved successfully.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return Result<bool>.Failure($"Risk Engine Profile Error: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Failed to save profile details to Risk Engine: {ex.Message}");
            }
        }

        public async Task<Result<ProjectRiskResponseDto>> AnalyzeProjectRisksAsync(Guid projectId)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Workspace)
                    .Include(p => p.TeamMembers)
                        .ThenInclude(m => m.User)
                    .Include(p => p.Taskitem)
                        .ThenInclude(t => t.Dependencies)
                    .FirstOrDefaultAsync(p => p.Id == projectId.ToString());

                if (project == null)
                    return Result<ProjectRiskResponseDto>.Failure("Project not found in database.");

                var teamLanguages = project.TeamMembers
                    .Where(m => m.User != null && m.User.Skills != null)
                    .SelectMany(m => m.User.Skills)
                    .Where(s => s.Equals("C#", StringComparison.OrdinalIgnoreCase) ||
                                s.Equals("Python", StringComparison.OrdinalIgnoreCase) ||
                                s.Equals("Dart", StringComparison.OrdinalIgnoreCase) ||
                                s.Equals("JavaScript", StringComparison.OrdinalIgnoreCase))
                    .Distinct()
                    .ToList();

                var requestDto = new ProjectRiskRequestDto
                {
                    ProjectId = project.Id.ToString(),
                    ProjectName = project.Name,
                    Description = project.Description ?? string.Empty,
                    ClientName = project.Workspace?.Name ?? string.Empty,

                    StartDate = project.StartDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    Deadline = project.EndDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),

                    EstimatedHours = project.Budget.HasValue ? 0 : 0,
                    BudgetUsd = project.Budget.HasValue ? (double)project.Budget.Value : 0.0,

                    Team = project.TeamMembers.Select(m => {
                        return new TeamMemberDto
                        {
                            Name = m.User != null ? $"{m.User.FirstName} {m.User.LastName}" : "Unknown Member",
                            Role = m.Role.ToString(),
                            Skills = m.User?.Skills ?? new List<string>(),
                            CurrentWorkloadPct = 0.0,
                            SeniorityYears = 0
                        };
                    }).ToList(),

                    TechStack = new TechStackDto
                    {
                        Languages = teamLanguages,
                        Frameworks = new List<string>(), 
                        Infrastructure = string.IsNullOrEmpty(project.RepositoryUrl) ? new List<string>() : new List<string> { project.RepositoryUrl },
                        ThirdPartyApis = new List<string>()
                    },

                    RequiredSkills = new List<string>(),
                    HasClearRequirements = (int)project.Status == 1, 
                    RequirementCompletenessPct = project.Taskitem.Any()
                        ? Math.Round((double)project.Taskitem.Count(t => (int)t.Status == 3) / project.Taskitem.Count * 100, 2)
                        : 0.0,

                    SimilarPastProjects = new List<string>(),
                    DependenciesCount = project.Taskitem.Sum(t => t.Dependencies?.Count ?? 0),
                    ThirdPartyIntegrationsCount = 0, 
                    InfrastructureReady = project.Taskitem.Any(t => t.TaskStartedAt.HasValue),
                    ClientResponsiveness = 0 
                };

                var client = _httpClientFactory.CreateClient("AI_Risk_Server");
                var response = await client.PostAsJsonAsync("api/v1/risk/analyze-project", requestDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ProjectRiskResponseDto>(_jsonOptions);
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
                var project = await _context.Projects
                    .Include(p => p.Taskitem)
                    .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);

                if (project == null)
                    return Result<ProjectRiskResponseDto>.Failure("Project not found in database.");

                if (project.Taskitem != null && project.Taskitem.Any())
                {
                    dto.TotalTasks = project.Taskitem.Count;
                    dto.OverdueTasks = project.Taskitem.Count(t => project.EndDate < DateTime.UtcNow && (int)t.Status != 2);
                }

                dto.SnapshotAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

                var payload = new
                {
                    project_id = dto.ProjectId,
                    snapshot_at = dto.SnapshotAt,
                    sprint_velocity = dto.SprintVelocity,
                    planned_velocity = dto.PlannedVelocity,
                    sprint_completion_rate = dto.SprintCompletionRate / 100.0,
                    overdue_tasks = dto.OverdueTasks,
                    total_tasks = dto.TotalTasks,
                    blocked_tasks = dto.BlockedTasks,
                    task_reassignment_count = dto.TaskReassignmentCount,
                    github_commits_last_7d = dto.GithubCommitsLast7d,
                    pr_open_count = dto.PrOpenCount,
                    pr_avg_review_hours = dto.PrAvgReviewHours,
                    deployment_failures_last_30d = dto.DeploymentFailuresLast30d,
                    qa_failure_rate = dto.QaFailureRate / 100.0,               
                    team_overtime_hours_avg = dto.TeamOvertimeHoursAvg,
                    team_absences_count = dto.TeamAbsencesCount,
                    negative_sentiment_score = dto.NegativeSentimentScore / 100.0, 
                    client_alignment_score = dto.ClientAlignmentScore,
                    client_response_delay_hours = dto.ClientResponseDelayHours,
                    unresolved_client_feedback = dto.UnresolvedClientFeedback
                };

                var client = _httpClientFactory.CreateClient("AI_Risk_Server");

                var response = await client.PostAsJsonAsync("/api/v1/risk/live-update", payload, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    var liveReport = await response.Content.ReadFromJsonAsync<ProjectRiskResponseDto>(_jsonOptions);
                    return liveReport != null
                        ? Result<ProjectRiskResponseDto>.Success(liveReport, "Live real-time risk metrics processed and broadcasted.")
                        : Result<ProjectRiskResponseDto>.Failure("Failed to deserialize live risk report.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return Result<ProjectRiskResponseDto>.Failure($"Live Risk Engine Error: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex)
            {
                return Result<ProjectRiskResponseDto>.Failure($"Failed to push live updates to Risk Engine: {ex.Message}");
            }
        }
    }
}