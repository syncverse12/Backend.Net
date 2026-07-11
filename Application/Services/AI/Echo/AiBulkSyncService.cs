using Microsoft.EntityFrameworkCore;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.Echo;
using SyncVerse.Application.Interfaces.AI.Echo;
using SyncVerse.Domain.Entities;
using SyncVerse.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace SyncVerse.Application.Services.AI.Echo
{
    public class AiBulkSyncService : IAiBulkSyncService
    {
        private readonly IAiEchoService _echoService;
        private readonly IServiceProvider _serviceProvider;

        public AiBulkSyncService(IAiEchoService echoService, IServiceProvider serviceProvider)
        {
            _echoService = echoService;
            _serviceProvider = serviceProvider;
        }

        public async System.Threading.Tasks.Task SyncSingleChangeToEchoAsync(Guid projectId, string title, string content, string type, string teamName = "")
        {
            try
            {
                var memoryDto = new EchoMemoryUploadDto
                {
                    ProjectId = projectId.ToString(),
                    TeamName = teamName,
                    MemoryType = type,
                    Title = title,
                    Content = content,
                    Author = "Backend_System",
                    Metadata = new { }
                };

                await _echoService.SaveProjectMemoryAutomatedAsync(memoryDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BulkSync Error] Failed to sync memory '{title}' for project {projectId}. Reason: {ex.Message}");
                throw;
            }
        }

        public async Task<Result<int>> SyncAllApplicationDataToEchoAsync(Guid projectId)
        {
            try
            {
                int syncedRecordsCount = 0;
                var context = _serviceProvider.GetRequiredService<DatabaseDbContext>();
                string projectIdStr = projectId.ToString();

                // 1. Fetch Root Project Data using explicit string ID
                var project = await context.Set<SyncVerse.Domain.Entities.Project>()
                    .FirstOrDefaultAsync(p => p.Id == projectIdStr);

                if (project == null)
                {
                    return Result<int>.Failure("Project not found in the database.");
                }

                string projectContent = $"Project Baseline Information: Name is '{project.Name}', " +
                                        $"Budget is {project.Budget} USD. " +
                                        $"The project officially starts on {project.StartDate:yyyy-MM-dd} " +
                                        $"and is scheduled to end on {project.EndDate:yyyy-MM-dd}.";

                await SyncSingleChangeToEchoAsync(projectId, $"Project Configuration Sync: {project.Name}", projectContent, "documentation", "Management_Team");
                syncedRecordsCount++;

                string? targetWorkspaceId = project.WorkspaceId;

                if (!string.IsNullOrEmpty(targetWorkspaceId))
                {
                    // 2. Sync Workspace Metadata explicitly
                    var workspace = await context.Set<Workspace>().FirstOrDefaultAsync(w => w.Id == targetWorkspaceId);
                    if (workspace != null)
                    {
                        string workspaceContent = $"Workspace '{workspace.Name}' Seed State: OrgCode: '{workspace.OrgCode}', Industry Sector: '{workspace.Industry ?? "General"}', Managed by User ID: '{workspace.CreatedByUserId}'.";

                        await SyncSingleChangeToEchoAsync(projectId, "Workspace Root Matrix Modified", workspaceContent, "documentation", "Management_Team");
                        syncedRecordsCount++;

                        // 3. Sync User-Workspace Bonds
                        var userWorkspaces = await context.Set<UserWorkspace>().Where(uw => uw.WorkspaceId == targetWorkspaceId).ToListAsync();
                        foreach (var uw in userWorkspaces)
                        {
                            string uwContent = $"User Workspace Bond Seed: User ID '{uw.UserId}' joined Workspace ID '{uw.WorkspaceId}' on {uw.JoinedAt:yyyy-MM-dd}.";
                            await SyncSingleChangeToEchoAsync(projectId, "Corporate Workspace Directory Update", uwContent, "documentation", "Management_Team");
                            syncedRecordsCount++;
                        }

                        // 4. Sync Corporate Teams
                        var teams = await context.Set<Domain.Entities.Team>().Where(t => t.WorkspaceId == targetWorkspaceId).ToListAsync();
                        foreach (var team in teams)
                        {
                            string dynamicTeamName = $"{team.Name ?? "Unknown"}_Team";

                            string teamContent = $"Team '{team.Name}' Baseline Data within Workspace ID '{team.WorkspaceId}'. Specialization: {team.Specialization}, Department: {team.Department}. Lead by: {team.TeamLeaderId ?? "None"}.";

                            await SyncSingleChangeToEchoAsync(projectId, "Workspace Department Team Created/Updated", teamContent, "documentation", dynamicTeamName);
                            syncedRecordsCount++;

                            // 5. Sync Department Team Members
                            var teamMembers = await context.Set<TeamMember>().Where(tm => tm.TeamId == team.Id).ToListAsync();
                            foreach (var tm in teamMembers)
                            {
                                string tmContent = $"Team Member Seed: User ID '{tm.UserId}' assigned to Department Team ID '{tm.TeamId}' with Role: {tm.Role}. Active Status: {tm.IsActive}.";

                                await SyncSingleChangeToEchoAsync(projectId, "Department Team Membership Change", tmContent, "documentation", dynamicTeamName);
                                syncedRecordsCount++;
                            }
                        }

                        // 6. Sync Virtual Sync Meetings
                        if (!string.IsNullOrEmpty(workspace.OrgCode))
                        {
                            var meetings = await context.Set<Meeting>().Where(m => m.OrgCode == workspace.OrgCode).ToListAsync();
                            foreach (var meeting in meetings)
                            {
                                string meetingContent = $"Meeting Hosted: Room '{meeting.RoomId}' under Organization Code '{meeting.OrgCode}'. Vivox Audio Channel: '{meeting.VivoxChannelName}'. Summary Notes: {meeting.Summary ?? "N/A"}. Decisions made: {meeting.Decisions ?? "N/A"}. Key Points: {meeting.KeyPoints ?? "N/A"}.";
                                await SyncSingleChangeToEchoAsync(projectId, "Virtual Sync Meeting Activity Log", meetingContent, "documentation", "Collaboration_Team");
                                syncedRecordsCount++;
                            }
                        }
                    }
                }

                // 7. Sync Project Board Categories
                var categories = await context.Set<TaskCategory>().ToListAsync();
                foreach (var cat in categories)
                {
                    string catContent = $"Task Category '{cat.Name}' Seed State by Owner User ID '{cat.UserId}'.";
                    await SyncSingleChangeToEchoAsync(projectId, "Task Board Taxonomy Changed", catContent, "documentation", "Product_Team");
                    syncedRecordsCount++;
                }

                // 8. Sync Project Task Items
                var tasks = await context.Set<TaskItem>().Where(t => t.ProjectId == projectIdStr && !t.IsDeleted).ToListAsync();
                foreach (var task in tasks)
                {
                    string taskContent = $"Task Seed Status: Title is '{task.Title}', Status: ({task.Status}), Due Date: {task.DueDate:yyyy-MM-dd}.";
                    await SyncSingleChangeToEchoAsync(projectId, "Task Board Synchronizer", taskContent, "documentation", "Development_Team");
                    syncedRecordsCount++;

                    // 9. Sync Task Attachments
                    var attachments = await context.Set<TaskAttachment>().Where(ta => ta.TaskId == task.Id).ToListAsync();
                    foreach (var attachment in attachments)
                    {
                        string attachContent = $"Attachment Seed: File '{attachment.FileName}' ({attachment.FileSize} bytes) associated with Task ID '{attachment.TaskId}'. Uploaded by User ID '{attachment.UploadedByUserId}' at {attachment.UploadedAt:yyyy-MM-dd HH:mm:ss}.";
                        await SyncSingleChangeToEchoAsync(projectId, "Task Asset Uploaded", attachContent, "documentation", "Development_Team");
                        syncedRecordsCount++;
                    }

                    // 10. Sync Task Dependencies
                    var dependencies = await context.Set<TaskDependency>().Where(td => td.TaskId == task.Id).ToListAsync();
                    foreach (var dependency in dependencies)
                    {
                        string depContent = $"Dependency Seed Mapping: Task ID '{dependency.TaskId}' now strictly depends on the completion of Task ID '{dependency.DependsOnTaskId}'.";
                        await SyncSingleChangeToEchoAsync(projectId, "Task Dependency Mapping", depContent, "documentation", "Development_Team");
                        syncedRecordsCount++;
                    }
                }

                // 11. Sync Project Milestones
                var milestones = await context.Set<Milestone>().Where(m => m.ProjectId == projectIdStr && !m.IsDeleted).ToListAsync();
                foreach (var milestone in milestones)
                {
                    string milestoneContent = $"Milestone Seed Status: Status details updated for Milestone ID {milestone.Id} on target project timeline.";
                    await SyncSingleChangeToEchoAsync(projectId, "Project Milestone Sync", milestoneContent, "documentation", "Management_Team");
                    syncedRecordsCount++;
                }

                // 12. Sync Project Roster Members
                var projectMembers = await context.Set<ProjectMember>().Where(pm => pm.ProjectId == projectIdStr).ToListAsync();
                foreach (var pm in projectMembers)
                {
                    string pmContent = $"Project Member Seed Status: User ID '{pm.UserId}' is linked to this project holding the Role code of '{pm.Role}'. Active Status: {pm.IsActive}. Permissions - Assign Tasks: {pm.CanAssignTasks}, Review Tasks: {pm.CanReviewTasks}, Edit Project: {pm.CanEditProject}.";
                    await SyncSingleChangeToEchoAsync(projectId, "Project Team Roster Update", pmContent, "documentation", "Management_Team");
                    syncedRecordsCount++;
                }

                return Result<int>.Success(syncedRecordsCount, $"Bulk synchronization completed successfully. Total records indexed: {syncedRecordsCount}");
            }
            catch (Exception ex)
            {
                return Result<int>.Failure($"Bulk synchronization pipeline failed: {ex.Message}");
            }
        }
    }
}