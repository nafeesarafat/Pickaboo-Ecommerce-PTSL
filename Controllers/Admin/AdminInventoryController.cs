using System.ComponentModel.DataAnnotations;
using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/inventory")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class AdminInventoryController : ControllerBase
{
    private readonly IAdminInventoryService _inventoryService;

    public AdminInventoryController(IAdminInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("overview")]
    [ProducesResponseType(typeof(ApiResponse<AdminInventoryOverviewDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
    {
        var overview = await _inventoryService.GetOverviewAsync(cancellationToken);
        return Ok(ApiResponse<AdminInventoryOverviewDto>.Ok(
            overview, "Inventory overview retrieved successfully."));
    }

    [HttpGet("{productId:int}/stock")]
    [ProducesResponseType(typeof(ApiResponse<AdminProductStockDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductStock(
        [FromRoute, Range(1, int.MaxValue)] int productId,
        CancellationToken cancellationToken)
    {
        var stock = await _inventoryService.GetProductStockAsync(productId, cancellationToken);
        return Ok(ApiResponse<AdminProductStockDto>.Ok(stock, "Product stock retrieved successfully."));
    }

    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AdminLowStockProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLowStock(
        [FromQuery, Range(0, int.MaxValue)] int threshold = AdminInventoryService.DefaultLowStockThreshold,
        CancellationToken cancellationToken = default)
    {
        var products = await _inventoryService.GetLowStockProductsAsync(threshold, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminLowStockProductDto>>.Ok(
            products, "Low-stock products retrieved successfully."));
    }

    [HttpGet("out-of-stock")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AdminProductStockDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOutOfStock(CancellationToken cancellationToken)
    {
        var products = await _inventoryService.GetOutOfStockProductsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminProductStockDto>>.Ok(
            products, "Out-of-stock products retrieved successfully."));
    }

    [HttpPost("{productId:int}/adjustments")]
    [ProducesResponseType(typeof(ApiResponse<AdminStockAdjustmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdjustStock(
        [FromRoute, Range(1, int.MaxValue)] int productId,
        [FromBody] AdjustStockRequestDto request,
        CancellationToken cancellationToken)
    {
        var adjustment = await _inventoryService.AdjustStockAsync(productId, request, cancellationToken);
        return Ok(ApiResponse<AdminStockAdjustmentDto>.Ok(
            adjustment, "Stock adjusted successfully."));
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AdminStockHistoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStockHistory(CancellationToken cancellationToken)
    {
        var history = await _inventoryService.GetStockHistoryAsync(null, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminStockHistoryDto>>.Ok(
            history, "Stock history retrieved successfully."));
    }

    [HttpGet("{productId:int}/history")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AdminStockHistoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductStockHistory(
        [FromRoute, Range(1, int.MaxValue)] int productId,
        CancellationToken cancellationToken)
    {
        var history = await _inventoryService.GetStockHistoryAsync(productId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminStockHistoryDto>>.Ok(
            history, "Product stock history retrieved successfully."));
    }
}
