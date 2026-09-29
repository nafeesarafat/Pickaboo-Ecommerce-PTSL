using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs.Admin;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly AppDbContext _context;

    public AdminDashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken)
    {
        var totalRevenue = await _context.Orders
            .Select(order => (decimal?)order.TotalAmount)
            .SumAsync(cancellationToken) ?? 0m;

        return new DashboardSummaryDto
        {
            TotalProducts = await _context.Products.CountAsync(cancellationToken),
            TotalCategories = await _context.Categories.CountAsync(cancellationToken),
            TotalOrders = await _context.Orders.CountAsync(cancellationToken),
            OrderStatus = await _context.Orders.CountAsync(
                order => order.OrderStatus != string.Empty, cancellationToken),
            TotalUsers = await _context.UserInformation.CountAsync(cancellationToken),
            PendingOrders = await _context.Orders.CountAsync(
                order => order.OrderStatus.ToLower() == "pending", cancellationToken),
            DeliveredOrders = await _context.Orders.CountAsync(
                order => order.OrderStatus.ToLower() == "delivered", cancellationToken),
            CancelledOrders = await _context.Orders.CountAsync(
                order => order.OrderStatus.ToLower() == "cancelled", cancellationToken),
            TotalRevenue = totalRevenue
        };
    }

    public async Task<IReadOnlyList<LowStockProductDto>> GetLowStockAsync(int threshold, CancellationToken cancellationToken)
    {
        if (threshold < 0) throw new ArgumentOutOfRangeException(nameof(threshold));
        return await _context.Products.AsNoTracking().Where(x => x.StockQuantity <= threshold)
            .OrderBy(x => x.StockQuantity).Select(x => new LowStockProductDto { ProductId = x.ProductId, ProductName = x.ProductName, StockQuantity = x.StockQuantity, Threshold = threshold }).ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RecentOrderDto>> GetRecentOrdersAsync(int limit, CancellationToken cancellationToken)
    {
        if (limit < 1) throw new ArgumentOutOfRangeException(nameof(limit));
        return await _context.Orders.AsNoTracking().OrderByDescending(x => x.OrderDate).Take(limit)
            .Select(x => new RecentOrderDto { OrderId = x.OrderId, UserName = x.User.UserName, OrderStatus = x.OrderStatus, OrderDate = x.OrderDate, TotalAmount = x.TotalAmount }).ToArrayAsync(cancellationToken);
    }

    public async Task<SalesOverviewDto> GetSalesOverviewAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken)
    {
        var query = _context.Orders.AsNoTracking().AsQueryable();
        if (fromDate.HasValue) query = query.Where(x => x.OrderDate >= fromDate);
        if (toDate.HasValue) query = query.Where(x => x.OrderDate <= toDate);
        return new SalesOverviewDto { FromDate = fromDate, ToDate = toDate, TotalOrders = await query.CountAsync(cancellationToken), TotalSales = await query.Select(x => (decimal?)x.TotalAmount).SumAsync(cancellationToken) ?? 0m };
    }

    public async Task<IReadOnlyList<TopSellingProductDto>> GetTopProductsAsync(int limit, CancellationToken cancellationToken)
    {
        if (limit < 1) throw new ArgumentOutOfRangeException(nameof(limit));
        return await _context.OrderItems.AsNoTracking().GroupBy(x => new { x.ProductId, x.ProductName })
            .Select(x => new TopSellingProductDto { ProductId = x.Key.ProductId, ProductName = x.Key.ProductName, UnitsSold = x.Sum(i => i.Quantity), Revenue = x.Sum(i => i.TotalAmount) })
            .OrderByDescending(x => x.UnitsSold).ThenByDescending(x => x.Revenue).Take(limit).ToArrayAsync(cancellationToken);
    }
}