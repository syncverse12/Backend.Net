using Graduation_Project.Domain.Enums;

namespace Graduation_Project.Application.DTOs.Tasks.Employee
{
    public class EmployeeTaskQuery
    {
        public TaskStatus? Status { get; set; }
        public TaskPriority? Priority { get; set; }
        public DateTime? FromDeadline { get; set; }
        public DateTime? ToDeadline { get; set; }
    }

}
