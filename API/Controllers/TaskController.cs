using Graduation_Project.Application.DTOs.Tasks;
using Graduation_Project.Application.Interfaces;
using Graduation_Project.Domain.Models;
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

    // CREATE
    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskDto dto)
    {

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _taskService.CreateAsync(dto, userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // GET
    [HttpGet("manager/tasks")]
    public async Task<IActionResult> GetMyTasks([FromQuery] TaskQuery query)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _taskService.GetManagerTasksAsync(userId, query);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // UPDATE
    [HttpPut("{taskId}")] 
    public async Task<IActionResult> Update(string taskId, UpdateTaskDto dto) 
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _taskService.UpdateAsync(taskId, dto, userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // DELETE 
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _taskService.DeleteAsync(id, userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // RESTORE 
    [HttpPut("{id}/restore")]
    public async Task<IActionResult> Restore(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _taskService.RestoreAsync(id, userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // ADD DEPENDENCY
    [Authorize(Policy = "ManagerPolicy")]
    [HttpPost("dependency")]
    public async Task<IActionResult> AddDependency(AddTaskDependencyDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _taskService.AddDependencyAsync(dto, userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // CONFIRM TASK
    [Authorize(Policy = "ManagerPolicy")]
    [HttpPut("{taskId}/confirm")]
    public async Task<IActionResult> Confirm(string taskId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _taskService.ConfirmTaskAsync(taskId, userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // REJECT TASK
    [Authorize(Policy = "ManagerPolicy")]
    [HttpPut("{taskId}/reject")]
    public async Task<IActionResult> Reject(string taskId, [FromBody] string comment)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _taskService.RejectTaskAsync(taskId, userId, comment);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // MANAGER DASHBOARD
    [Authorize(Policy = "ManagerPolicy")]
    [HttpGet("manager/dashboard")]
    public async Task<IActionResult> GetManagerDashboard()
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _taskService.GetManagerDashboardAsync(managerId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

}