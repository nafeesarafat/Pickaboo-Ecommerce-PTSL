using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs.Order;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private const decimal DefaultShippingFee = 10m;

    public OrderService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    // --------------------------------------------------------------- API 23
    public async Task<OrderResponseDto> PlaceOrderAsync(PlaceOrderRequestDto request, CancellationToken cancellationToken)
    {
        // The authenticated identity is the authority for ownership; the client value is discarded.
        int userId = _currentUser.GetRequiredUserId();

        if (string.IsNullOrWhiteSpace(request.ShippingAddress))
        {
            throw new BadRequestException("ShippingAddress is required.");
        }

        // Load the user's cart (tracked so it can be removed after checkout).
        var cartItems = await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .ToListAsync(cancellationToken);

        if (cartItems.Count == 0)
        {
            throw new BadRequestException("The cart is empty.");
        }

        // Load the products (tracked so stock changes are persisted).
        var productIds = cartItems.Select(ci => ci.ProductId).Distinct().ToList();

        var products = await _context.Products
            .Where(p => productIds.Contains(p.ProductId))
            .ToListAsync(cancellationToken);

        // Verify availability and stock for every cart item using database data.
        foreach (var cartItem in cartItems)
        {
            var product = products.FirstOrDefault(p => p.ProductId == cartItem.ProductId);
            if (product is null)
            {
                throw new NotFoundException($"Product with id {cartItem.ProductId} was not found.");
            }

            if (!product.IsActive)
            {
                throw new NotFoundException($"Product '{product.ProductName}' is no longer available.");
            }

            if (product.StockQuantity < cartItem.Quantity)
            {
                throw new ConflictException(
                    $"Insufficient stock for '{product.ProductName}'. " +
                    $"Available: {product.StockQuantity}, requested: {cartItem.Quantity}.");
            }
        }

        // Calculate amounts — the database price is the authority.
        decimal subTotal = 0m;
        foreach (var cartItem in cartItems)
        {
            var product = products.First(p => p.ProductId == cartItem.ProductId);
            subTotal += product.Price * cartItem.Quantity;
        }

        decimal shippingFee = await GetShippingFeeAsync(cancellationToken);
        decimal finalAmount = subTotal + shippingFee;

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var order = new Order
            {
                UserId = userId,
                ShippingAddress = request.ShippingAddress,
                City = request.City,
                Country = request.Country,
                PhoneNumber = request.PhoneNumber,
                OrderStatus = "Pending",
                OrderDate = DateTime.UtcNow,
                SubTotal = subTotal,
                ShippingFee = shippingFee,
                TotalAmount = finalAmount
            };
            _context.Orders.Add(order);
            _context.ActivityLogs.Add(new ActivityLog
            {
                Order = order,
                UserId = userId,
                Action = "Created",
                Description = "Order was placed.",
                Timestamp = order.OrderDate
            });

            // Create order items and reduce stock — all using database prices.
            foreach (var cartItem in cartItems)
            {
                var product = products.First(p => p.ProductId == cartItem.ProductId);

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    Quantity = cartItem.Quantity,
                    TotalAmount = product.Price * cartItem.Quantity
                });

                product.StockQuantity -= cartItem.Quantity;
            }

            // Clear the cart as part of the same transaction.
            _context.CartItems.RemoveRange(cartItems);

            var notificationSettings = await _context.NotificationSettings
                .AsNoTracking()
                .Where(settings => settings.UserId == null)
                .OrderBy(settings => settings.SettingsId)
                .FirstOrDefaultAsync(cancellationToken);
            if (notificationSettings?.OrderAlerts ?? true)
            {
                _context.Notifications.Add(new Notification
                {
                    Type = "Order",
                    Message = "A new order was placed.",
                    Order = order,
                    UserId = userId,
                    CreatedDate = order.OrderDate
                });
            }

            if (notificationSettings?.StockAlerts ?? true)
            {
                foreach (var product in products.Where(item => item.StockQuantity is >= 0 and <= AdminInventoryService.DefaultLowStockThreshold))
                {
                    _context.Notifications.Add(new Notification
                    {
                        Type = "Stock",
                        Message = product.StockQuantity == 0
                            ? $"Product '{product.ProductName}' is out of stock."
                            : $"Product '{product.ProductName}' is low on stock ({product.StockQuantity} remaining).",
                        ProductId = product.ProductId,
                        CreatedDate = order.OrderDate
                    });
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return MapToOrderResponse(order);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    // --------------------------------------------------------------- API 24
    public async Task<IReadOnlyList<OrderResponseDto>> GetMyOrdersAsync(CancellationToken cancellationToken)
    {
        int userId = _currentUser.GetRequiredUserId();

        var orders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);

        return orders.Select(MapToOrderResponse).ToList().AsReadOnly();
    }

    // --------------------------------------------------------------- API 25
    public async Task<OrderDetailsResponseDto> GetOrderDetailsAsync(int orderId, CancellationToken cancellationToken)
    {
        int userId = _currentUser.GetRequiredUserId();

        // Ownership is verified inside the query so a foreign order both yields 404.
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId, cancellationToken)
            ?? throw new NotFoundException($"Order with id {orderId} was not found.");

        decimal unitPrice = order.OrderItems.FirstOrDefault()?.Price ?? 0m;

        return new OrderDetailsResponseDto
        {
            OrderId = order.OrderId,
            UserId = order.UserId,
            ShippingAddress = order.ShippingAddress,
            City = order.City ?? string.Empty,
            Country = order.Country ?? string.Empty,
            PhoneNumber = order.PhoneNumber ?? string.Empty,
            OrderStatus = order.OrderStatus,
            OrderDate = order.OrderDate,
            UpdatedDate = order.UpdatedDate ?? order.OrderDate,
            UnitPrice = unitPrice,
            TotalAmount = order.TotalAmount,
            SubTotal = order.SubTotal,
            ShippingFee = order.ShippingFee
        };
    }

    // --------------------------------------------------------------- API 26
    public async Task<OrderStatusResponseDto> GetOrderStatusAsync(int orderId, CancellationToken cancellationToken)
    {
        int userId = _currentUser.GetRequiredUserId();

        var result = await _context.Orders
            .AsNoTracking()
            .Where(o => o.OrderId == orderId && o.UserId == userId)
            .Select(o => new OrderStatusResponseDto
            {
                OrderId = o.OrderId,
                OrderStatus = o.OrderStatus,
                UpdateDate = o.UpdatedDate ?? o.OrderDate
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Order with id {orderId} was not found.");

        return result;
    }

    // --------------------------------------------------------------- API 27
    public async Task<OrderStatusResponseDto> CancelOrderAsync(int orderId, CancellationToken cancellationToken)
    {
        int userId = _currentUser.GetRequiredUserId();

        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId, cancellationToken)
            ?? throw new NotFoundException($"Order with id {orderId} was not found.");

        // Only Pending orders can be cancelled. Delivered (or any other terminal status)
        // cannot be reverted to Cancelled.
        if (order.OrderStatus != "Pending")
        {
            throw new ConflictException(
                $"Order cannot be cancelled. Current status: {order.OrderStatus}. " +
                $"Only orders with status 'Pending' can be cancelled.");
        }

        order.OrderStatus = "Cancelled";
        order.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new OrderStatusResponseDto
        {
            OrderId = order.OrderId,
            OrderStatus = order.OrderStatus,
            UpdateDate = order.UpdatedDate ?? order.OrderDate
        };
    }

    private async Task<decimal> GetShippingFeeAsync(CancellationToken cancellationToken)
    {
        var settings = await _context.StoreSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        return settings?.DefaultShippingFee ?? DefaultShippingFee;
    }

    private static OrderResponseDto MapToOrderResponse(Order order)
    {
        return new OrderResponseDto
        {
            OrderId = order.OrderId,
            UserId = order.UserId,
            ShippingAddress = order.ShippingAddress,
            City = order.City ?? string.Empty,
            Country = order.Country ?? string.Empty,
            PhoneNumber = order.PhoneNumber ?? string.Empty,
            OrderStatus = order.OrderStatus,
            OrderDate = order.OrderDate,
            UpdatedDate = order.UpdatedDate ?? order.OrderDate,
            SubTotal = order.SubTotal,
            ShippingFee = order.ShippingFee,
            FinalAmount = order.TotalAmount
        };
    }
}
