using Discounts.Application.DTOs.Categories;
using Discounts.Application.Services.Interfaces;
using Discounts.Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.API.Controllers;


[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/categories")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // ── Public ────────────────────────────────────────────────────────────────

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveCategories(CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetActiveCategoriesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategoryById(int id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    // ── Admin ─────────────────────────────────────────────────────────────────

    [HttpGet("all")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetAllCategories(
        [FromQuery] bool includeInactive = false, 
        CancellationToken cancellationToken = default)
    {
        var result = await _categoryService.GetAllCategoriesAsync(includeInactive, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequestDto request, 
        CancellationToken cancellationToken)
    {
        var result = await _categoryService.CreateCategoryAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetCategoryById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> UpdateCategory(
        int id, 
        [FromBody] UpdateCategoryRequestDto request, 
        CancellationToken cancellationToken)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
    {
        await _categoryService.DeleteCategoryAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/activate")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> ActivateCategory(int id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.ActivateCategoryAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:int}/deactivate")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeactivateCategory(int id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.DeactivateCategoryAsync(id, cancellationToken);
        return Ok(result);
    }
}




























