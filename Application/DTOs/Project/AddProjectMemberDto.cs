using SyncVerse.Domain.Enums;

namespace SyncVerse.Application.DTOs.Project
{
    public class AddProjectMemberDto
    {
        public string ProjectId { get; set; } = null!;
        public string UserEmail { get; set; } = null!;

        public ProjectRole Role { get; set; }

        // ✅ Permissions
        public bool CanAssignTasks { get; set; } = false;
        public bool CanReviewTasks { get; set; } = false;
        public bool CanEditProject { get; set; } = false;
    }
}