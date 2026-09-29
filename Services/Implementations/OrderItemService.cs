using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs.Order;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Services;

public class OrderItemService : IOrderItemService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public OrderItemService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<OrderItemResponseDto>> GetOrderItemsAsync(
        int orderId,
        CancellationToken cancellationToken)
    {
        int userId = _currentUser.GetRequiredUserId();

        bool orderExists = await _context.Orders
            .AnyAsync(order => order.OrderId == orderId && order.UserId == userId, cancellationToken);

        if (!orderExists)
        {
            throw new NotFoundException($"Order with id {orderId} was not found.");
        }

        var items = await _context.OrderItems
            .AsNoTracking()
            .Where(item => item.OrderId == orderId)
            .Include(item => item.Product)
            .OrderBy(item => item.OrderItemId)
            .ToListAsync(cancellationToken);

        return items.Select(item => new OrderItemResponseDto
        {
            OrderId = item.OrderId,
            OrderItemId = item.OrderItemId,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Price = item.Price,
            Quantity = item.Quantity,
            TotalAmount = item.TotalAmount
        }).ToList().AsReadOnly();
    }

    public async Task<OrderItemDetailsResponseDto> GetOrderItemAsync(
        int orderItemId,
        CancellationToken cancellationToken)
    {
        int userId = _currentUser.GetRequiredUserId();

        var item = await GetOwnedOrderItemAsync(orderItemId, userId, cancellationToken, tracked: false);
        return MapDetails(item);
    }

    public async Task<OrderItemDetailsResponseDto> CreateOrderItemAsync(
        int orderId,
        CreateOrderItemRequestDto request,
        CancellationToken cancellationToken)
    {
        int userId = _currentUser.GetRequiredUserId();

        var order = await _context.Orders
            .FirstOrDefaultAsync(order => order.OrderId == orderId && order.UserId == userId, cancellationToken)
            ?? throw new NotFoundException($"Order with id {orderId} was not found.");

        EnsureOrderIsEditable(order);
        ValidateQuantity(request.Quantity);

        var product = await _context.Products
            .FirstOrDefaultAsync(product => product.ProductId == request.ProductId, cancellationToken)
            ?? throw new NotFoundException($"Product with id {request.ProductId} was not found.");

        var item = new OrderItem
        {
            OrderId = order.OrderId,
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            Price = product.Price,
            Quantity = request.Quantity,
            TotalAmount = product.Price * request.Quantity
        };

        _context.OrderItems.Add(item);
        await RecalculateOrderTotalAsync(order, cancellationToken, additionalTotal: item.TotalAmount);
        await _context.SaveChangesAsync(cancellationToken);

        item.Product = product;
        return MapDetails(item);
    }

    public async Task<OrderItemDetailsResponseDto> UpdateOrderItemAsync(
        int orderItemId,
        UpdateOrderItemRequestDto request,
        CancellationToken cancellationToken)
    {
        int userId = _currentUser.GetRequiredUserId();
        ValidateQuantity(request.StockQuantity);

        var item = await GetOwnedOrderItemAsync(orderItemId, userId, cancellationToken, tracked: true);
        EnsureOrderIsEditable(item.Order);

        var product = await _context.Products
            .FirstOrDefaultAsync(product => product.ProductId == item.ProductId, cancellationToken)
            ?? throw new NotFoundException($"Product with id {item.ProductId} was not found.");

        item.ProductName = product.ProductName;
        item.Price = product.Price;
        item.Quantity = request.StockQuantity;
        item.TotalAmount = product.Price * request.StockQuantity;

        await RecalculateOrderTotalAsync(
            item.Order,
            cancellationToken,
            excludedOrderItemId: item.OrderItemId,
            additionalTotal: item.TotalAmount);
        await _context.SaveChangesAsync(cancellationToken);

        return MapDetails(item);
    }

    public async Task DeleteOrderItemAsync(int orderItemId, CancellationToken cancellationToken)
    {
        int userId = _currentUser.GetRequiredUserId();
        var item = await GetOwnedOrderItemAsync(orderItemId, userId, cancellationToken, tracked: true);
        EnsureOrderIsEditable(item.Order);

        _context.OrderItems.Remove(item);
        await RecalculateOrderTotalAsync(
            item.Order,
            cancellationToken,
            excludedOrderItemId: item.OrderItemId);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<OrderItem> GetOwnedOrderItemAsync(
        int orderItemId,
        int userId,
        CancellationToken cancellationToken,
        bool tracked)
    {
        IQueryable<OrderItem> query = _context.OrderItems
            .Where(item => item.OrderItemId == orderItemId && item.Order.UserId == userId)
            .Include(item => item.Order)
            .Include(item => item.Product);

        if (!tracked)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Order item with id {orderItemId} was not found.");
    }

    private async Task RecalculateOrderTotalAsync(
        Order order,
        CancellationToken cancellationToken,
        int? excludedOrderItemId = null,
        decimal additionalTotal = 0m)
    {
        order.SubTotal = await _context.OrderItems
            .Where(item => item.OrderId == order.OrderId
                && (!excludedOrderItemId.HasValue || item.OrderItemId != excludedOrderItemId.Value))
            .SumAsync(item => (decimal?)item.TotalAmount, cancellationToken) ?? 0m;
        order.SubTotal += additionalTotal;
        order.TotalAmount = order.SubTotal + order.ShippingFee;
        order.UpdatedDate = DateTime.UtcNow;
    }

    private static void ValidateQuantity(int quantity)
    {
        if (quantity < 1)
        {
            throw new BadRequestException("Quantity must be greater than zero.");
        }
    }

    private static void EnsureOrderIsEditable(Order order)
    {
        if (order.OrderStatus.Equals("Shipped", StringComparison.OrdinalIgnoreCase)
            || order.OrderStatus.Equals("Delivered", StringComparison.OrdinalIgnoreCase)
            || order.OrderStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException(
                $"Order items cannot be changed when the order status is '{order.OrderStatus}'.");
        }
    }

    private static OrderItemDetailsResponseDto MapDetails(OrderItem item)
    {
        return new OrderItemDetailsResponseDto
        {
            OrderItemId = item.OrderItemId,
            OrderId = item.OrderId,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            UnitPrice = item.Price,
            StockQuantity = item.Quantity,
            TotalAmount = item.TotalAmount
        };
    }
}