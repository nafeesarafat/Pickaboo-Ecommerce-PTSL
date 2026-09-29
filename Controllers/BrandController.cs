using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController, Route("api/brands")]
public class BrandController : ControllerBase
{
    private readonly IBrandService _service;
    public BrandController(IBrandService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? brandId, [FromQuery] string? brandName, [FromQuery] string? description, [FromQuery] string? imageUrl, [FromQuery] bool? isActive, CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<BrandDto>>.Ok(await _service.GetAllAsync(brandId, brandName, description, imageUrl, isActive, cancellationToken), "Brands retrieved successfully."));
}
