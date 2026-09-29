using System.ComponentModel.DataAnnotations;
using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/brands")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class AdminBrandController : ControllerBase
{
    private readonly IAdminBrandService _brandService;

    public AdminBrandController(IAdminBrandService brandService)
    {
        _brandService = brandService;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AdminBrandDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute, Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        var brand = await _brandService.GetBrandAsync(id, cancellationToken);
        return Ok(ApiResponse<AdminBrandDto>.Ok(brand, "Brand retrieved successfully."));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AdminBrandDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add(
        [FromBody] AddBrandRequestDto request,
        CancellationToken cancellationToken)
    {
        var brand = await _brandService.AddBrandAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = brand.BrandId },
            ApiResponse<AdminBrandDto>.Created(brand, "Brand created successfully."));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AdminBrandDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        [FromRoute, Range(1, int.MaxValue)] int id,
        [FromBody] UpdateBrandRequestDto request,
        CancellationToken cancellationToken)
    {
        var brand = await _brandService.UpdateBrandAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminBrandDto>.Ok(brand, "Brand updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute, Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        await _brandService.DeleteBrandAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<AdminBrandDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        [FromRoute, Range(1, int.MaxValue)] int id,
        [FromBody] UpdateBrandStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var brand = await _brandService.UpdateBrandStatusAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminBrandDto>.Ok(brand, "Brand status updated successfully."));
    }
}
