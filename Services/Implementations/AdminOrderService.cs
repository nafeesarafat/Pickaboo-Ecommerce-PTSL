using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Services;

public class AdminOrderService : IAdminOrderService
{
    private static readonly HashSet<string> SupportedStatuses = new(StringComparer.Ordinal)
    {
        "Pending",
        "Confirmed",
        "Processing",
        "Shipped",
        "Delivered",
        "Cancelled"
    };

    private readonly AppDbContext _context;

    public AdminOrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AdminOrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken)
    {
        return await _context.Orders
            .AsNoTracking()
            .OrderByDescending(order => order.OrderDate)
            .Select(order => new AdminOrderDto
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                UserName = order.User.UserName,
                TotalAmount = order.TotalAmount,
                OrderStatus = order.OrderStatus,
                OrderDate = order.OrderDate
            })
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminPendingOrderDto>> GetPendingOrdersAsync(CancellationToken cancellationToken)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(order => order.OrderStatus == "Pending")
            .OrderByDescending(order => order.OrderDate)
            .Select(order => new AdminPendingOrderDto
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                UserName = order.User.UserName,
                OrderStatus = order.OrderStatus,
                OrderDate = order.OrderDate
            })
            .ToArrayAsync(cancellationToken);
    }

    public async Task<AdminOrderDetailsDto> GetOrderDetailsAsync(
        int orderId,
        CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(item => item.User)
            .Include(item => item.OrderItems)
            .SingleOrDefaultAsync(item => item.OrderId == orderId, cancellationToken)
            ?? throw new NotFoundException($"Order with id {orderId} was not found.");

        return new AdminOrderDetailsDto
        {
            OrderId = order.OrderId,
            UserName = order.User.UserName,
            UserId = order.UserId,
            OrderStatus = order.OrderStatus,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            ShippingAddress = order.ShippingAddress,
            SubTotal = order.SubTotal,
            FinalAmount = order.TotalAmount,
            ShippingFee = order.ShippingFee,
            OrderItems = order.OrderItems.Select(item => new AdminOrderItemDto
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                UnitPrice = item.Price,
                SubTotal = item.TotalAmount,
                Quantity = item.Quantity
            }).ToArray()
        };
    }

    public async Task<AdminOrderStatusDto> UpdateOrderStatusAsync(
        int orderId,
        UpdateAdminOrderStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var status = request.OrderStatus.Trim();
        if (!SupportedStatuses.Contains(status))
        {
            throw new BadRequestException(
                "OrderStatus must be one of: Pending, Confirmed, Processing, Shipped, Delivered, Cancelled.");
        }

        var order = await _context.Orders
            .SingleOrDefaultAsync(item => item.OrderId == orderId, cancellationToken)
            ?? throw new NotFoundException($"Order with id {orderId} was not found.");

        order.OrderStatus = status;
        order.UpdatedDate = DateTime.UtcNow;
        RecordOrderActivity(order, "Status updated", $"Order status changed to {status}.");
        await _context.SaveChangesAsync(cancellationToken);

        return ToStatusDto(order.OrderId, order.OrderStatus, order.UpdatedDate.Value);
    }

    public async Task<AdminDeliveryStatusDto> UpdateDeliveryStatusAsync(
        int orderId,
        UpdateDeliveryStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        if (orderId <= 0)
        {
            throw new BadRequestException("A valid order id is required.");
        }

        string status = request.Status.Trim();
        if (status.Length == 0 || request.UpdatedDate == default)
        {
            throw new BadRequestException("A delivery status and valid updated date are required.");
        }

        var order = await _context.Orders
            .SingleOrDefaultAsync(item => item.OrderId == orderId, cancellationToken)
            ?? throw new NotFoundException($"Order with id {orderId} was not found.");

        order.DeliveryStatus = status;
        order.UpdatedDate = request.UpdatedDate;
        RecordOrderActivity(order, "Delivery status updated", $"Delivery status changed to {status}.");
        await _context.SaveChangesAsync(cancellationToken);

        return new AdminDeliveryStatusDto
        {
            OrderId = order.OrderId,
            Status = order.DeliveryStatus,
            UpdatedDate = order.UpdatedDate.Value
        };
    }

    public async Task<AdminOrderCancelledDto> CancelOrderAsync(int orderId, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .SingleOrDefaultAsync(item => item.OrderId == orderId, cancellationToken)
            ?? throw new NotFoundException($"Order with id {orderId} was not found.");

        order.OrderStatus = "Cancelled";
        order.UpdatedDate = DateTime.UtcNow;
        RecordOrderActivity(order, "Cancelled", "Order was cancelled.");
        await _context.SaveChangesAsync(cancellationToken);

        return new AdminOrderCancelledDto
        {
            OrderId = order.OrderId,
            OrderStatus = order.OrderStatus,
            CancelledDate = order.UpdatedDate.Value
        };
    }

    private static AdminOrderStatusDto ToStatusDto(int orderId, string status, DateTime updatedDate)
    {
        return new AdminOrderStatusDto
        {
            OrderId = orderId,
            OrderStatus = status,
            UpdatedDate = updatedDate
        };
    }

    private void RecordOrderActivity(Order order, string action, string description)
    {
        _context.ActivityLogs.Add(new ActivityLog
        {
            OrderId = order.OrderId,
            Action = action,
            Description = description,
            Timestamp = DateTime.UtcNow
        });
    }
}
