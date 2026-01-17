using Graduation_Project.Application.Common.Pagination;
using Graduation_Project.Domain.Enums;

public class TaskQuery : PaginationQuery
{
    public bool? IsCompleted { get; set; }
    public bool? IsDeleted { get; set; }
    public string? Search { get; set; }
    public TaskSortBy SortBy { get; set; } = TaskSortBy.Newest;
    public TaskPriority? Priority { get; set; }

}