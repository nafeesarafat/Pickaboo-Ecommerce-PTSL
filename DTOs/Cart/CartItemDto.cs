namespace Ecommerce.Api.DTOs.Cart;

/// <summary>
/// Read-only projection of a single cart line item, joining the <see cref="Models.CartItem"/>
/// with its <see cref="Models.Product"/>. Prices always come from the database and are never
/// trusted from the client.
/// </summary>
public class CartItemDto
{
    public int CartItemId { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    /// <summary>Quantity of this product currently held in the cart.</summary>
    public int StockQuantity { get; set; }

    /// <summary>Current unit price of the product, retrieved from the database (decimal precision).</summary>
    public decimal Price { get; set; }

    /// <summary>Unit price expressed as an integer, used for the integer line total.</summary>
    public int UnitPrice { get; set; }

    /// <summary>Line total: <see cref="UnitPrice"/> multiplied by the cart quantity.</summary>
    public int TotalPrice { get; set; }
}
