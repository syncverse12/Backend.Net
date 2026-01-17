using Graduation_Project.API.Authorization.Policies;
using Graduation_Project.Application.Common.Pagination;
using Graduation_Project.Application.DTOs.Tasks;
using Graduation_Project.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_Project.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        //CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CreateTaskDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _taskService.CreateAsync(dto, userId);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        //GET
        [HttpGet("my-tasks")]
        public async Task<IActionResult> GetMyTasks([FromQuery] TaskQuery query)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var result = await _taskService.GetMyTasksAsync(userId, query);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        //UPDATE
        [Authorize(Policy = Policies.TaskOwner)]
        [HttpPut("{taskId}")]
        public async Task<IActionResult> Update(string id, UpdateTaskDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _taskService.UpdateAsync(id, dto, userId);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        //DELETE
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _taskService.DeleteAsync(id, userId);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        //RESTORE
        [HttpPut("{id:int}/restore")]
        public async Task<IActionResult> Restore(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _taskService.RestoreAsync(id, userId);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        //ADD DEPENDENCY
        [Authorize(Policy = "ManagerPolicy")]
        [HttpPost("dependency")]
        public async Task<IActionResult> AddDependency(AddTaskDependencyDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _taskService.AddDependencyAsync(dto, userId);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

    }
}
