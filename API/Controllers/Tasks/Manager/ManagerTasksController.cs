using Graduation_Project.Application.DTOs.Project.Manager;
using Graduation_Project.Application.DTOs.Tasks;
using Graduation_Project.Application.DTOs.Tasks.Manager;
using Graduation_Project.Application.Interfaces.Task.Manager;
using Graduation_Project.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

    // CREATE TASK (Manager)
    [HttpPost]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<IActionResult> Create(CreateTaskDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _taskService.CreateAsync(dto, userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // GET MANAGER TASKS
    [HttpGet("manager/tasks")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<IActionResult> GetManagerTasks([FromQuery] TaskQuery query)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _taskService.GetManagerTasksAsync(userId, query);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // UPDATE TASK
    [HttpPut("{taskId}")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<IActionResult> Update(string taskId, UpdateTaskDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _taskService.UpdateAsync(taskId, dto, userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // DELETE TASK (Soft Delete)
    [HttpDelete("{id}")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _taskService.DeleteAsync(id, userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // RESTORE TASK
    [HttpPut("{id}/restore")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<IActionResult> Restore(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _taskService.RestoreAsync(id, userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // ADD DEPENDENCY
    [HttpPost("dependency")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<IActionResult> AddDependency(AddTaskDependencyDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _taskService.AddDependencyAsync(dto, userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // CONFIRM TASK
    [HttpPut("{taskId}/confirm")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<IActionResult> Confirm(string taskId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _taskService.ConfirmTaskAsync(taskId, userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // REJECT TASK
    [HttpPut("{taskId}/reject")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<IActionResult> Reject(string taskId, [FromBody] string comment)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _taskService.RejectTaskAsync(taskId, userId, comment);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // MANAGER DASHBOARD
    [HttpGet("manager/dashboard")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<IActionResult> GetManagerDashboard()
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _taskService.GetManagerDashboardAsync(managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("filter")]
    public async Task<IActionResult> Filter(TaskFilterDto filter)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _taskService.FilterTasksAsync(filter, managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("dashboard/{projectId}")]
    public async Task<IActionResult> Dashboard(string projectId)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _taskService.GetDashboardAsync(projectId, managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
