using System.ComponentModel.DataAnnotations;
using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/categories")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class AdminCategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public AdminCategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AdminCategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllCategoriesForAdminAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminCategoryDto>>.Ok(
            categories, "Categories retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AdminCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute, Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        var category = await _categoryService.GetCategoryForAdminAsync(id, cancellationToken);
        return Ok(ApiResponse<AdminCategoryDto>.Ok(category, "Category retrieved successfully."));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AdminCategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add(
        [FromBody] AddCategoryRequestDto request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryService.AddCategoryAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = category.CategoryId },
            ApiResponse<AdminCategoryDto>.Created(category, "Category created successfully."));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AdminCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        [FromRoute, Range(1, int.MaxValue)] int id,
        [FromBody] UpdateCategoryRequestDto request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryService.UpdateCategoryAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminCategoryDto>.Ok(category, "Category updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute, Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        await _categoryService.DeleteCategoryAsync(id, cancellationToken);
        return NoContent();
    }
}