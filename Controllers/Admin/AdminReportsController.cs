using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/reports")]
[Authorize(Roles = "Admin")]
public class AdminReportsController : ControllerBase
{
    private readonly IReportService _service;

    public AdminReportsController(IReportService service) => _service = service;

    [HttpGet("sales")]
    public async Task<IActionResult> GetSales([FromQuery] ReportDateRangeQueryDto query, CancellationToken cancellationToken) =>
        Ok(ApiResponse<SalesReportDto>.Ok(await _service.GetSalesAsync(query, cancellationToken), "Sales report retrieved."));

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue([FromQuery] ReportDateRangeQueryDto query, CancellationToken cancellationToken) =>
        Ok(ApiResponse<RevenueReportDto>.Ok(await _service.GetRevenueAsync(query, cancellationToken), "Revenue report retrieved."));

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders([FromQuery] ReportDateRangeQueryDto query, CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<OrderReportDto>>.Ok(await _service.GetOrdersAsync(query, cancellationToken), "Order report retrieved."));

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts([FromQuery] ReportDateRangeQueryDto query, CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<ProductReportDto>>.Ok(await _service.GetProductsAsync(query, cancellationToken), "Product report retrieved."));

    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers([FromQuery] ReportDateRangeQueryDto query, CancellationToken cancellationToken) =>
        Ok(ApiResponse<CustomerReportDto>.Ok(await _service.GetCustomersAsync(query, cancellationToken), "Customer report retrieved."));

    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventory(CancellationToken cancellationToken) =>
        Ok(ApiResponse<InventoryReportDto>.Ok(await _service.GetInventoryAsync(cancellationToken), "Inventory report retrieved."));

    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments([FromQuery] ReportDateRangeQueryDto query, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PaymentReportDto>.Ok(await _service.GetPaymentsAsync(query, cancellationToken), "Payment report retrieved."));

    [HttpGet("discounts")]
    public async Task<IActionResult> GetDiscounts([FromQuery] ReportDateRangeQueryDto query, CancellationToken cancellationToken) =>
        Ok(ApiResponse<DiscountReportDto>.Ok(await _service.GetDiscountsAsync(query, cancellationToken), "Discount report retrieved."));

    [HttpGet("sales-by-category")]
    public async Task<IActionResult> GetSalesByCategory(CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<SalesByCategoryDto>>.Ok(await _service.GetSalesByCategoryAsync(cancellationToken), "Category sales report retrieved."));

    [HttpGet("sales-by-brand")]
    public async Task<IActionResult> GetSalesByBrand(CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<SalesByBrandDto>>.Ok(await _service.GetSalesByBrandAsync(cancellationToken), "Brand sales report retrieved."));

    [HttpGet("sales-by-date")]
    public async Task<IActionResult> GetSalesByDate([FromQuery] ReportDateRangeQueryDto query, CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<SalesByDateDto>>.Ok(await _service.GetSalesByDateAsync(query, cancellationToken), "Date sales report retrieved."));

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] ExportReportRequestDto request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<ExportReportDto>.Ok(await _service.ExportAsync(request, cancellationToken), "Report exported successfully."));

    [HttpGet("download/{fileName}")]
    public IActionResult Download(string fileName)
    {
        string path = _service.GetExportPath(fileName);
        string contentType = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".csv" => "text/csv",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };

        return PhysicalFile(path, contentType, fileName);
    }
}
