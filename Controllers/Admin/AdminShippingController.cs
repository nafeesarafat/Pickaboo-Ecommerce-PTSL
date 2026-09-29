using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/shipping")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class AdminShippingController : ControllerBase
{
    private readonly IAdminShippingService _shippingService;

    public AdminShippingController(IAdminShippingService shippingService)
    {
        _shippingService = shippingService;
    }

    [HttpGet("methods")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AdminShippingMethodDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMethods(CancellationToken cancellationToken)
    {
        var methods = await _shippingService.GetMethodsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminShippingMethodDto>>.Ok(
            methods, "Shipping methods retrieved successfully."));
    }

    [HttpGet("zones")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AdminShippingZoneDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetZones(CancellationToken cancellationToken)
    {
        var zones = await _shippingService.GetZonesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminShippingZoneDto>>.Ok(
            zones, "Shipping zones retrieved successfully."));
    }

    [HttpGet("zones/{zoneId:int}/charge")]
    [ProducesResponseType(typeof(ApiResponse<AdminShippingChargeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetZoneCharge(
        [FromRoute] int zoneId,
        CancellationToken cancellationToken)
    {
        var charge = await _shippingService.GetZoneChargeAsync(zoneId, cancellationToken);
        return Ok(ApiResponse<AdminShippingChargeDto>.Ok(
            charge, "Shipping charge retrieved successfully."));
    }

    [HttpGet("delivery-areas")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AdminDeliveryAreaDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeliveryAreas(CancellationToken cancellationToken)
    {
        var areas = await _shippingService.GetDeliveryAreasAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminDeliveryAreaDto>>.Ok(
            areas, "Delivery areas retrieved successfully."));
    }
}
