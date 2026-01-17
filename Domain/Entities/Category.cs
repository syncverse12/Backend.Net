using Graduation_Project.Domain.Common;
using Graduation_Project.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = null!;
    public string UserId { get; set; } = null!;

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
