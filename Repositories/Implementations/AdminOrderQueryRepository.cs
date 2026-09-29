using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Repositories;

public class AdminOrderQueryRepository : IAdminOrderQueryRepository
{
    private readonly AppDbContext _context;

    public AdminOrderQueryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AdminOrderViewDto>> GetByStatusAsync(
        string status,
        CancellationToken cancellationToken)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(order => order.OrderStatus == status)
            .OrderByDescending(order => order.OrderDate)
            .ThenByDescending(order => order.OrderId)
            .Select(AdminOrderCustomerMappings.OrderView)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<PagedResultDto<AdminOrderViewDto>> SearchAsync(
        AdminOrderSearchRequestDto request,
        CancellationToken cancellationToken)
    {
        var orders = _context.Orders.AsNoTracking().AsQueryable();
        string? keyword = string.IsNullOrWhiteSpace(request.Keyword) ? null : request.Keyword.Trim();

        if (keyword is not null)
        {
            bool hasOrderId = int.TryParse(keyword, out int orderId);
            orders = orders.Where(order =>
                order.User.UserName.Contains(keyword)
                || (hasOrderId && order.OrderId == orderId));
        }

        if (!string.IsNullOrWhiteSpace(request.OrderStatus))
        {
            string status = request.OrderStatus.Trim();
            orders = orders.Where(order => order.OrderStatus == status);
        }

        if (request.FromDate.HasValue)
        {
            orders = orders.Where(order => order.OrderDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            orders = orders.Where(order => order.OrderDate <= request.ToDate.Value);
        }

        int totalItems = await orders.CountAsync(cancellationToken);
        var items = await orders
            .OrderByDescending(order => order.OrderDate)
            .ThenByDescending(order => order.OrderId)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(AdminOrderCustomerMappings.OrderView)
            .ToArrayAsync(cancellationToken);

        return new PagedResultDto<AdminOrderViewDto>
        {
            Items = items,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
