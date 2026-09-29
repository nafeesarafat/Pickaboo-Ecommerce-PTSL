using System.ComponentModel.DataAnnotations;
using Ecommerce.Api.DTOs.Category;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Produces("application/json")]
[AllowAnonymous]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>API 14 - Returns active categories and their active product counts.</summary>
    /// <response code="200">The public category list, which may be empty.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CategoryListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetCategoriesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CategoryListDto>>.Ok(
            categories, "Categories retrieved successfully."));
    }

    /// <summary>API 15 - Returns one active category and its active product count.</summary>
    /// <param name="id">The category identifier.</param>
    /// <response code="200">The requested category.</response>
    /// <response code="400">The id is not a positive integer.</response>
    /// <response code="404">No active category exists with the given id.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategory(
        [FromRoute, Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<CategoryDetailsDto>.Ok(category, "Category retrieved successfully."));
    }

    /// <summary>API 16 - Returns active products and their brand details for an active category.</summary>
    /// <param name="id">The category identifier.</param>
    /// <response code="200">The category products, which may be an empty list.</response>
    /// <response code="400">The id is not a positive integer.</response>
    /// <response code="404">No active category exists with the given id.</response>
    [HttpGet("{id:int}/products")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CategoryProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryProducts(
        [FromRoute, Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        var products = await _categoryService.GetCategoryProductsAsync(id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CategoryProductDto>>.Ok(
            products, "Category products retrieved successfully."));
    }
}
