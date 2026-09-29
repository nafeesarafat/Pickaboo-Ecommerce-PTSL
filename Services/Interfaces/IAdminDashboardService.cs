using Ecommerce.Api.DTOs.Admin;

namespace Ecommerce.Api.Services;

public interface IAdminDashboardService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<LowStockProductDto>> GetLowStockAsync(int threshold, CancellationToken cancellationToken);
    Task<IReadOnlyList<RecentOrderDto>> GetRecentOrdersAsync(int limit, CancellationToken cancellationToken);
    Task<SalesOverviewDto> GetSalesOverviewAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken);
    Task<IReadOnlyList<TopSellingProductDto>> GetTopProductsAsync(int limit, CancellationToken cancellationToken);
}