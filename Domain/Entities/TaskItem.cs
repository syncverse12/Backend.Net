using Graduation_Project.Domain.Common;
using Graduation_Project.Domain.Entities;
using Graduation_Project.Domain.Enums;


public class TaskItem : BaseEntity
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    public TaskStatus Status { get; set; }

    public string CreatedByUserId { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;

    public string AssignedToUserId { get; set; } = null!;
    public User AssignedToUser { get; set; } = null!;

    public DateTime? DueDate { get; set; }

    public string? CategoryId { get; set; }
    public Category? Category { get; set; }
    public virtual ICollection<TaskDependency> Dependencies { get; set; } = new List<TaskDependency>();
    public virtual ICollection<TaskDependency> DependentTasks { get; set; } = new List<TaskDependency>();

    public TaskPriority Priority { get; set; }

    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByUserId { get; set; }
    public User? ReviewedByUser { get; set; }

    public string? ReviewComment { get; set; }

    public string? SubmissionLink { get; set; }
    public string? SubmissionNotes { get; set; }
    public DateTime? SubmittedAt { get; set; }

    public string? ProjectId { get; set; }
    public Project? Project { get; set; }

    public string? MilestoneId { get; set; }
    public Milestone? Milestone { get; set; }

    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}
