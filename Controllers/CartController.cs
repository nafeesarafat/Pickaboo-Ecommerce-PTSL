using Ecommerce.Api.DTOs.Cart;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/cart")]
[Produces("application/json")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// API 17 - Returns the authenticated user's cart. Product data is joined from the catalog
    /// and prices are taken from the database, not from the request.
    /// </summary>
    /// <response code="200">The user's cart (possibly an empty list).</response>
    /// <response code="401">No valid access token was supplied.</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CartItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CartItemDto>>>> GetCart(CancellationToken cancellationToken)
    {
        var cart = await _cartService.GetCartAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CartItemDto>>.Ok(cart, "Cart retrieved successfully."));
    }

    /// <summary>
    /// API 18 - Returns the number of items in the authenticated user's cart.
    /// Uses an efficient count query and never loads the cart rows themselves.
    /// </summary>
    /// <response code="200">The cart item count for the authenticated user.</response>
    /// <response code="401">No valid access token was supplied.</response>
    [HttpGet("count")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CartCountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<CartCountDto>>> GetCartCount(CancellationToken cancellationToken)
    {
        var count = await _cartService.GetCartCountAsync(cancellationToken);
        return Ok(ApiResponse<CartCountDto>.Ok(count, "Cart count retrieved successfully."));
    }

    /// <summary>
    /// API 19 - Adds a product to the authenticated user's cart. If the product is already in the
    /// cart, the line quantity is increased instead of creating a duplicate row.
    /// </summary>
    /// <param name="request">The product and quantity to add. Client-supplied UserId is ignored.</param>
    /// <response code="201">The cart line item that was created or updated.</response>
    /// <response code="400">The request is missing, malformed or the quantity is invalid.</response>
    /// <response code="401">No valid access token was supplied.</response>
    /// <response code="404">The product does not exist or is not active.</response>
    /// <response code="409">The requested quantity exceeds the available stock.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CartItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<CartItemDto>>> AddToCart(
        [FromBody] AddCartItemRequestDto request,
        CancellationToken cancellationToken)
    {
        var item = await _cartService.AddToCartAsync(request, cancellationToken);
        return Created("/api/cart", ApiResponse<CartItemDto>.Created(item, "Product added to cart."));
    }

    /// <summary>
    /// API 20 - Updates the quantity of a cart item owned by the authenticated user.
    /// The client-supplied UpdatedDate is ignored; ownership is enforced via the JWT identity.
    /// </summary>
    /// <param name="cartItemId">The cart item identifier from the route.</param>
    /// <response code="200">The updated cart line item.</response>
    /// <response code="400">The quantity is invalid.</response>
    /// <response code="401">No valid access token was supplied.</response>
    /// <response code="404">The cart item does not exist for the authenticated user.</response>
    /// <response code="409">The requested quantity exceeds the available stock.</response>
    [HttpPut("{cartItemId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CartItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<CartItemDto>>> UpdateCart(
        [FromRoute] int cartItemId,
        [FromBody] UpdateCartItemRequestDto request,
        CancellationToken cancellationToken)
    {
        var item = await _cartService.UpdateCartAsync(cartItemId, request, cancellationToken);
        return Ok(ApiResponse<CartItemDto>.Ok(item, "Cart item updated."));
    }

    /// <summary>
    /// API 21 - Removes a single cart item owned by the authenticated user.
    /// </summary>
    /// <param name="cartItemId">The cart item identifier from the route.</param>
    /// <response code="200">The cart item was removed.</response>
    /// <response code="401">No valid access token was supplied.</response>
    /// <response code="404">The cart item does not exist for the authenticated user.</response>
    [HttpDelete("{cartItemId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> RemoveFromCart(
        [FromRoute] int cartItemId,
        CancellationToken cancellationToken)
    {
        await _cartService.RemoveFromCartAsync(cartItemId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Cart item removed from cart."));
    }

    /// <summary>
    /// API 22 - Clears every cart item belonging to the authenticated user.
    /// </summary>
    /// <response code="200">The cart was cleared (or was already empty).</response>
    /// <response code="401">No valid access token was supplied.</response>
    [HttpDelete]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> ClearCart(CancellationToken cancellationToken)
    {
        await _cartService.ClearCartAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Cart cleared successfully."));
    }
}
