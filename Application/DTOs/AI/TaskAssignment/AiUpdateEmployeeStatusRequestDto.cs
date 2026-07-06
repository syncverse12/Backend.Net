using System.ComponentModel.DataAnnotations;

namespace SyncVerse.Application.DTOs.AI.TaskAssignment
{
    public class AiUpdateEmployeeStatusRequestDto
    {
        [Required]
        public int Employee_id { get; set; }

        public int Active_tasks { get; set; }

        public int Availability_score { get; set; }

        public double Past_success_rate { get; set; }
    }
}
