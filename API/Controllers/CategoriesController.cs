using Graduation_Project.Application.DTOs.Category;
using Graduation_Project.Application.Interfaces;
using Graduation_Project.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryTaskService _categoryService;

    public CategoriesController(ICategoryTaskService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
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
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await _categoryService.DeleteAsync(id, userId));
    }
}
