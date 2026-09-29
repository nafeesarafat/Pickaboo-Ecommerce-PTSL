using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs.Cart;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Services;

public class CartService : ICartService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CartService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    // --------------------------------------------------------------- API 17
    public async Task<IReadOnlyList<CartItemDto>> GetCartAsync(CancellationToken cancellationToken)
    {
        int userId = _currentUser.GetRequiredUserId();

        var rows = await _context.CartItems
            .AsNoTracking()
            .Where(ci => ci.UserId == userId)
            .Select(ci => new
            {
                ci.CartItemId,
                ci.UserId,
                ci.ProductId,
                ProductName = ci.Product.ProductName,
                Quantity = ci.Quantity,
                Price = ci.Product.Price
            })
            .OrderBy(x => x.ProductName)
            .ToListAsync(cancellationToken);

        var result = rows.Select(row => new CartItemDto
        {
            CartItemId = row.CartItemId,
            UserId = row.UserId,
            ProductId = row.ProductId,
            ProductName = row.ProductName,
            StockQuantity = row.Quantity,
            Price = row.Price,
            UnitPrice = (int)row.Price,
            TotalPrice = (int)row.Price * row.Quantity
        })
        .ToList();

        return result;
    }

    // --------------------------------------------------------------- API 18
    public async Task<CartCountDto> GetCartCountAsync(CancellationToken cancellationToken)
    {
        int userId = _currentUser.GetRequiredUserId();

        int count = await _context.CartItems
            .AsNoTracking()
            .CountAsync(ci => ci.UserId == userId, cancellationToken);

        return new CartCountDto { UserId = userId, Count = count };
    }

    // --------------------------------------------------------------- API 19
    public async Task<CartItemDto> AddToCartAsync(AddCartItemRequestDto request, CancellationToken cancellationToken)
    {
        // The authenticated identity is the authority for ownership; the client value is discarded.
        int userId = _currentUser.GetRequiredUserId();

        long productIdLong = request.ProductId;
        if (productIdLong <= 0)
        {
            throw new BadRequestException("ProductId must be a positive integer.");
        }

        int productId = productIdLong <= int.MaxValue
            ? (int)productIdLong
            : throw new NotFoundException($"Product with id {productIdLong} was not found.");

        if (request.StockQuantity <= 0)
        {
            throw new BadRequestException("StockQuantity must be a positive integer.");
        }

        var product = await _context.Products
            .AsNoTracking()
            .Where(p => p.ProductId == productId)
            .Select(p => new { p.ProductId, p.ProductName, p.Price, p.StockQuantity, p.IsActive })
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            throw new NotFoundException($"Product with id {productId} was not found.");
        }

        if (!product.IsActive)
        {
            throw new NotFoundException($"Product with id {productId} is not available.");
        }

        // Collapse duplicate lines for the same user+product instead of inserting a second row.
        var existing = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId, cancellationToken);

        int quantity = request.StockQuantity;

        if (existing is not null)
        {
            int newQuantity = existing.Quantity + quantity;
            if (newQuantity > product.StockQuantity)
            {
                throw new ConflictException(
                    "The total quantity for this product would exceed the available stock.");
            }

            existing.Quantity = newQuantity;
        }
        else
        {
            if (quantity > product.StockQuantity)
            {
                throw new ConflictException("The requested quantity exceeds the available stock.");
            }

            existing = new CartItem { UserId = userId, ProductId = productId, Quantity = quantity };
            _context.CartItems.Add(existing);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(existing.CartItemId, userId, product.ProductId, product.ProductName, existing.Quantity, product.Price);
    }

    // --------------------------------------------------------------- API 20
    public async Task<CartItemDto> UpdateCartAsync(
        int cartItemId,
        UpdateCartItemRequestDto request,
        CancellationToken cancellationToken)
    {
        int userId = _currentUser.GetRequiredUserId();

        // Ownership is verified inside the query: a missing or foreign-owned item both yield 404,
        // so neither the existence nor the details of another user's cart are leaked.
        var existing = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.UserId == userId, cancellationToken)
            ?? throw new NotFoundException($"Cart item with id {cartItemId} was not found.");

        int quantity = request.StockQuantity;
        if (quantity <= 0)
        {
            throw new BadRequestException("StockQuantity must be a positive integer.");
        }

        var product = await _context.Products
            .AsNoTracking()
            .Where(p => p.ProductId == existing.ProductId)
            .Select(p => new { p.ProductName, p.Price, p.StockQuantity, p.IsActive })
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null || !product.IsActive)
        {
            throw new NotFoundException("The product associated with this cart item is no longer available.");
        }

        if (quantity > product.StockQuantity)
        {
            throw new ConflictException("The requested quantity exceeds the available stock.");
        }

        // The client-supplied UpdatedDate is intentionally ignored; the entity has no timestamp column.
        existing.Quantity = quantity;
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(existing.CartItemId, userId, existing.ProductId, product.ProductName, existing.Quantity, product.Price);
    }

    // --------------------------------------------------------------- API 21
    public async Task RemoveFromCartAsync(int cartItemId, CancellationToken cancellationToken)
    {
        int userId = _currentUser.GetRequiredUserId();

        var existing = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.UserId == userId, cancellationToken)
            ?? throw new NotFoundException($"Cart item with id {cartItemId} was not found.");

        _context.CartItems.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
    }

    // --------------------------------------------------------------- API 22
    public async Task ClearCartAsync(CancellationToken cancellationToken)
    {
        int userId = _currentUser.GetRequiredUserId();

        var items = await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .ToListAsync(cancellationToken);

        if (items.Count != 0)
        {
            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private static CartItemDto MapToDto(
        int cartItemId,
        int userId,
        int productId,
        string productName,
        int quantity,
        decimal price)
    {
        int unitPrice = (int)price;
        return new CartItemDto
        {
            CartItemId = cartItemId,
            UserId = userId,
            ProductId = productId,
            ProductName = productName,
            StockQuantity = quantity,
            Price = price,
            UnitPrice = unitPrice,
            TotalPrice = unitPrice * quantity
        };
    }
}
