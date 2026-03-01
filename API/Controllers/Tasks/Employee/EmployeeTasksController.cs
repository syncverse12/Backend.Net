using SyncVerse.Application.DTOs.Tasks.Employee;
using SyncVerse.Application.Interfaces.Tasks.Employee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


[Authorize(Policy = "EmployeeOnly")]
[ApiController]
[Route("api/employee/tasks")]
public class EmployeeTasksController : ControllerBase
{
    private readonly IEmployeeTaskService _employeeTaskService;

    public EmployeeTasksController(IEmployeeTaskService employeeTaskService)
    {
        _employeeTaskService = employeeTaskService;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyTasks([FromQuery] TaskQuery query)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _employeeTaskService.GetMyTasksAsync(userId, query);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{taskId}")]
    [Authorize(Policy = "TaskOwner")]
    public async Task<IActionResult> GetTaskDetails(string taskId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _employeeTaskService.GetTaskDetailsAsync(taskId, userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{taskId}/start")]
    [Authorize(Policy = "TaskOwner")]
    public async Task<IActionResult> StartTask(string taskId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _employeeTaskService.StartTaskAsync(taskId, userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{taskId}/submit")]
    [Authorize(Policy = "TaskOwner")]
    public async Task<IActionResult> SubmitTask(string taskId, [FromBody] SubmitTaskDto submitDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _employeeTaskService.SubmitTaskAsync(taskId, userId, submitDto);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}