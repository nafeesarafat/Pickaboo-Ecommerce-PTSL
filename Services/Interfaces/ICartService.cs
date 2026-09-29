using Ecommerce.Api.DTOs.Cart;

namespace Ecommerce.Api.Services;

/// <summary>
/// Cart business logic for roadmap APIs 17-22. Every authenticated operation resolves the
/// user from the JWT via <see cref="ICurrentUserService"/> and never trusts a client-supplied
/// user id. Product prices are always read from the database.
/// </summary>
public interface ICartService
{
    /// <summary>API 17 - returns the authenticated user's cart, joined with product data.</summary>
    Task<IReadOnlyList<CartItemDto>> GetCartAsync(CancellationToken cancellationToken);

    /// <summary>API 18 - returns the number of items in the authenticated user's cart.</summary>
    Task<CartCountDto> GetCartCountAsync(CancellationToken cancellationToken);

    /// <summary>API 19 - adds a product to the authenticated user's cart (or merges into an existing line).</summary>
    Task<CartItemDto> AddToCartAsync(AddCartItemRequestDto request, CancellationToken cancellationToken);

    /// <summary>API 20 - updates the quantity of a cart item owned by the authenticated user.</summary>
    Task<CartItemDto> UpdateCartAsync(int cartItemId, UpdateCartItemRequestDto request, CancellationToken cancellationToken);

    /// <summary>API 21 - removes a single cart item owned by the authenticated user.</summary>
    Task RemoveFromCartAsync(int cartItemId, CancellationToken cancellationToken);

    /// <summary>API 22 - removes every cart item belonging to the authenticated user.</summary>
    Task ClearCartAsync(CancellationToken cancellationToken);
}
