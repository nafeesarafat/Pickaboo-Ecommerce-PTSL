using Ecommerce.Api.DTOs.Admin;

namespace Ecommerce.Api.Services;

public interface IReportService
{
    Task<SalesReportDto> GetSalesAsync(ReportDateRangeQueryDto query, CancellationToken cancellationToken);
    Task<RevenueReportDto> GetRevenueAsync(ReportDateRangeQueryDto query, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrderReportDto>> GetOrdersAsync(ReportDateRangeQueryDto query, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductReportDto>> GetProductsAsync(ReportDateRangeQueryDto query, CancellationToken cancellationToken);
    Task<CustomerReportDto> GetCustomersAsync(ReportDateRangeQueryDto query, CancellationToken cancellationToken);
    Task<InventoryReportDto> GetInventoryAsync(CancellationToken cancellationToken);
    Task<PaymentReportDto> GetPaymentsAsync(ReportDateRangeQueryDto query, CancellationToken cancellationToken);
    Task<DiscountReportDto> GetDiscountsAsync(ReportDateRangeQueryDto query, CancellationToken cancellationToken);
    Task<IReadOnlyList<SalesByCategoryDto>> GetSalesByCategoryAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<SalesByBrandDto>> GetSalesByBrandAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<SalesByDateDto>> GetSalesByDateAsync(ReportDateRangeQueryDto query, CancellationToken cancellationToken);
    Task<ExportReportDto> ExportAsync(ExportReportRequestDto request, CancellationToken cancellationToken);
    string GetExportPath(string fileName);
}
