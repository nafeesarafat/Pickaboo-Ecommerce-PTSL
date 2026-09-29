using System.ComponentModel.DataAnnotations;
using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminDashboardService _dashboardService;
    private readonly IAdminProductService _productService;

    public AdminController(IAdminDashboardService dashboardService, IAdminProductService productService)
    {
        _dashboardService = dashboardService;
        _productService = productService;
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var summary = await _dashboardService.GetDashboardSummaryAsync(cancellationToken);
        return Ok(ApiResponse<DashboardSummaryDto>.Ok(summary, "Dashboard summary retrieved successfully."));
    }

    [HttpGet("products")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AdminProductResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllProducts(CancellationToken cancellationToken)
    {
        var products = await _productService.GetAllProductsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminProductResponseDto>>.Ok(
            products, "Products retrieved successfully."));
    }

    [HttpGet("products/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AdminProductDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductDetails(
        [FromRoute, Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductDetailsAsync(id, cancellationToken);
        return Ok(ApiResponse<AdminProductDetailsDto>.Ok(product, "Product retrieved successfully."));
    }

    [HttpPost("products")]
    [ProducesResponseType(typeof(ApiResponse<AdminProductResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddProduct(
        [FromBody] AddProductRequestDto request,
        CancellationToken cancellationToken)
    {
        var product = await _productService.AddProductAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetProductDetails),
            new { id = product.ProductId },
            ApiResponse<AdminProductResponseDto>.Created(product, "Product created successfully."));
    }

    [HttpPut("products/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AdminProductResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(
        [FromRoute, Range(1, int.MaxValue)] int id,
        [FromBody] UpdateProductRequestDto request,
        CancellationToken cancellationToken)
    {
        var product = await _productService.UpdateProductAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminProductResponseDto>.Ok(product, "Product updated successfully."));
    }

    [HttpDelete("products/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(
        [FromRoute, Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        await _productService.DeleteProductAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("dashboard/low-stock")]
    public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 5, CancellationToken cancellationToken = default)
        => Ok(ApiResponse<IReadOnlyList<LowStockProductDto>>.Ok(await _dashboardService.GetLowStockAsync(threshold, cancellationToken), "Low-stock products retrieved successfully."));

    [HttpGet("dashboard/recent-orders")]
    public async Task<IActionResult> GetRecentOrders([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
        => Ok(ApiResponse<IReadOnlyList<RecentOrderDto>>.Ok(await _dashboardService.GetRecentOrdersAsync(limit, cancellationToken), "Recent orders retrieved successfully."));

    [HttpGet("dashboard/sales-overview")]
    public async Task<IActionResult> GetSalesOverview([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, CancellationToken cancellationToken)
        => Ok(ApiResponse<SalesOverviewDto>.Ok(await _dashboardService.GetSalesOverviewAsync(fromDate, toDate, cancellationToken), "Sales overview retrieved successfully."));

    [HttpGet("dashboard/top-products")]
    public async Task<IActionResult> GetTopProducts([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
        => Ok(ApiResponse<IReadOnlyList<TopSellingProductDto>>.Ok(await _dashboardService.GetTopProductsAsync(limit, cancellationToken), "Top-selling products retrieved successfully."));
}