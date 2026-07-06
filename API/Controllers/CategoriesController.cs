using SyncVerse.Application.DTOs.TaskCategory;
using SyncVerse.Application.Interfaces.Task.Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

//[Authorize]
[ApiController]
[Route("api/task-categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryTaskService _categoryService;

    public CategoriesController(ICategoryTaskService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskCategoryDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await _categoryService.CreateAsync(dto, userId));
    }

    [HttpGet]
    public async Task<IActionResult> GetMyCategories()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await _categoryService.GetMyCategoriesAsync(userId));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await _categoryService.DeleteAsync(id, userId));
    }
}
