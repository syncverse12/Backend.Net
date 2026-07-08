using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SyncVerse.Application.Interfaces.AI.Echo;
using SyncVerse.Domain.Entities;
using System.Security.Claims;


namespace SyncVerse.Persistence.Interceptors
{
    public class AiMemoryInterceptor : SaveChangesInterceptor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor; 

        public AiMemoryInterceptor(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
        {
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.Entity != null && (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
                .ToList();

            using var scope = _serviceProvider.CreateScope();
            var aiSyncService = scope.ServiceProvider.GetService<IAiBulkSyncService>();

            if (aiSyncService != null && entries.Any())
            {
                var user = _httpContextAccessor.HttpContext?.User;

                string userName = user?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown_User";

                string userRole = user?.FindFirst(ClaimTypes.Role)?.Value
                                  ?? "Live_System";

                string liveTeamName = $"{userRole}_Live_Team";

                foreach (var entry in entries)
                {
                    var entity = entry.Entity;
                    if (entity == null) continue;

                    string title = string.Empty;
                    string content = string.Empty;
                    string type = "documentation";
                    string? projectId = null;

                    if (entity is Project project)
                    {
                        projectId = project.Id;
                        title = $"Project Configuration Sync: {project.Name}";
                        type = "documentation";
                        content = entry.State == EntityState.Deleted || project.IsDeleted
                            ? $"Project '{project.Name}' has been deleted from the system by user '{userName}'."
                            : $"Project {entry.State} by '{userName}': Name is '{project.Name}', Budget: {project.Budget}.";
                    }

                    else if (entity is TaskItem task)
                    {
                        projectId = task.ProjectId;
                        title = $"Task Board Synchronizer";
                        type = "task";
                        content = entry.State == EntityState.Deleted || task.IsDeleted
                            ? $"Task has been removed from the project board by user '{userName}'."
                            : $"Task {entry.State} by '{userName}': Title is '{task.Title}', Status: ({task.Status}), Due Date: {task.DueDate:yyyy-MM-dd}.";
                    }

                    else if (entity is Milestone milestone)
                    {
                        projectId = milestone.ProjectId;
                        title = $"Project Milestone Sync";
                        type = "documentation";
                        content = entry.State == EntityState.Deleted || milestone.IsDeleted
                            ? $"Milestone has been deleted from the timeline by user '{userName}'."
                            : $"Milestone {entry.State} by '{userName}': Status details updated for Milestone ID {milestone.Id}.";
                    }

                    else if (entity is ProjectMember projectMember)
                    {
                        projectId = projectMember.ProjectId;
                        title = $"Project Team Roster Update";
                        type = "management";
                        content = $"Project Member Assignment Changed by '{userName}' ({entry.State}): User ID '{projectMember.UserId}' linked with Role: {projectMember.Role}.";
                    }

                    else if (entity is TaskAttachment attachment)
                    {
                        var relatedTask = context.Set<TaskItem>().FirstOrDefault(t => t.Id == attachment.TaskId);
                        projectId = relatedTask?.ProjectId;

                        title = $"Task Asset Uploaded";
                        type = "task";
                        content = $"Attachment {entry.State} by '{userName}': File '{attachment.FileName}' associated with Task ID '{attachment.TaskId}'.";
                    }

                    else if (entity is TaskDependency dependency)
                    {
                        var relatedTask = context.Set<TaskItem>().FirstOrDefault(t => t.Id == dependency.TaskId);
                        projectId = relatedTask?.ProjectId;

                        title = $"Task Dependency Mapping";
                        type = "task";
                        content = $"Dependency {entry.State} managed by '{userName}': Task ID '{dependency.TaskId}' strictly depends on Task ID '{dependency.DependsOnTaskId}'.";
                    }

                    else if (entity is Team team)
                    {
                        var firstProjectInWorkspace = context.Set<SyncVerse.Domain.Entities.Project>().FirstOrDefault(p => p.WorkspaceId == team.WorkspaceId && !p.IsDeleted);
                        projectId = firstProjectInWorkspace?.Id;

                        title = $"Workspace Department Team Created/Updated";
                        type = "architecture";
                        content = $"Team '{team.Name}' structure altered by '{userName}' ({entry.State}) within Workspace ID '{team.WorkspaceId}'.";
                    }

                    else if (entity is TeamMember teamMember)
                    {
                        var relatedTeam = context.Set<Team>().FirstOrDefault(t => t.Id == teamMember.TeamId);
                        if (relatedTeam != null)
                        {
                            var firstProjectInWorkspace = context.Set<SyncVerse.Domain.Entities.Project>().FirstOrDefault(p => p.WorkspaceId == relatedTeam.WorkspaceId && !p.IsDeleted);
                            projectId = firstProjectInWorkspace?.Id;
                        }

                        title = $"Department Team Membership Change";
                        type = "management";
                        content = $"Team Member action by '{userName}' ({entry.State}): User ID '{teamMember.UserId}' assignment changed for Team ID '{teamMember.TeamId}'.";
                    }

                    else if (entity is Meeting meeting)
                    {
                        var relatedWorkspace = context.Set<Workspace>().FirstOrDefault(w => w.OrgCode == meeting.OrgCode);
                        if (relatedWorkspace != null)
                        {
                            var firstProjectInWorkspace = context.Set<SyncVerse.Domain.Entities.Project>().FirstOrDefault(p => p.WorkspaceId == relatedWorkspace.Id && !p.IsDeleted);
                            projectId = firstProjectInWorkspace?.Id;
                        }

                        title = $"Virtual Sync Meeting Activity Log";
                        type = "meeting";
                        content = entry.State == EntityState.Deleted
                            ? $"Meeting Room '{meeting.RoomId}' session log destroyed by '{userName}'."
                            : $"Meeting Session Logged by '{userName}': Room '{meeting.RoomId}' under Organization Code '{meeting.OrgCode}'.";
                    }

                    else if (entity is Workspace workspace)
                    {
                        var firstProjectInWorkspace = context.Set<SyncVerse.Domain.Entities.Project>().FirstOrDefault(p => p.WorkspaceId == workspace.Id && !p.IsDeleted);
                        projectId = firstProjectInWorkspace?.Id;

                        title = $"Workspace Root Matrix Modified";
                        type = "architecture";
                        content = $"Workspace '{workspace.Name}' modified by '{userName}' ({entry.State}).";
                    }

                    else if (entity is UserWorkspace uw)
                    {
                        var firstProjectInWorkspace = context.Set<SyncVerse.Domain.Entities.Project>().FirstOrDefault(p => p.WorkspaceId == uw.WorkspaceId && !p.IsDeleted);
                        projectId = firstProjectInWorkspace?.Id;

                        title = $"Corporate Workspace Directory Update";
                        type = "management";
                        content = $"User-Workspace Bound modified by '{userName}' ({entry.State}) for User ID '{uw.UserId}'.";
                    }

                    else if (entity is TaskCategory category)
                    {
                        var sampleTask = context.Set<TaskItem>().FirstOrDefault(t => t.CategoryId == category.Id);
                        projectId = sampleTask?.ProjectId;

                        title = $"Task Board Taxonomy Changed";
                        type = "task";
                        content = $"Task Category '{category.Name}' {entry.State} by '{userName}'.";
                    }

                    if (!string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(projectId))
                    {
                        if (Guid.TryParse(projectId, out Guid parsedProjectId))
                        {
                            _ = aiSyncService.SyncSingleChangeToEchoAsync(parsedProjectId, title, content, type, liveTeamName);
                        }
                    }
                }
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}