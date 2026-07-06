using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SyncVerse.Application.DTOs.AI.TaskAssignment
{
    public class AiAddEmployeeRequestDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Track { get; set; } = string.Empty;

        [Required]
        public List<string> Skills { get; set; } = new();

        [Required]
        public string Level { get; set; } = "Junior";

        public int Active_tasks { get; set; } = 0;

        public int Availability_score { get; set; } = 100;

        public double Past_success_rate { get; set; } = 0.85;
    }
}
