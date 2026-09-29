using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Order;

/// <summary>
/// Request body for placing an order (checkout). The authenticated user's cart is
/// checked out; no product or price information from the client is trusted.
/// </summary>
public class PlaceOrderRequestDto
{
    /// <summary>
    /// Client-supplied value is ignored. The authenticated user is always taken from the JWT.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Client-supplied value is ignored. Products are taken from the cart, not from this field.
    /// </summary>
    public int ProductId { get; set; }

    [Required(ErrorMessage = "ShippingAddress is required.")]
    [MaxLength(500, ErrorMessage = "ShippingAddress must not exceed 500 characters.")]
    public string ShippingAddress { get; set; } = string.Empty;

    /// <summary>
    /// Client-supplied value is ignored. Quantities are taken from the cart.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Client-supplied value is ignored. The unit price is always read from the database.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Client-supplied value is ignored. SubTotal is calculated on the server.
    /// </summary>
    public decimal SubTotal { get; set; }

    /// <summary>
    /// Client-supplied value is ignored. ShippingFee is calculated on the server.
    /// </summary>
    public decimal ShippingFee { get; set; }

    /// <summary>
    /// Client-supplied value is ignored. FinalAmount is calculated on the server.
    /// </summary>
    public decimal FinalAmount { get; set; }

    [Required(ErrorMessage = "PhoneNumber is required.")]
    [MaxLength(20, ErrorMessage = "PhoneNumber must not exceed 20 characters.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required.")]
    [MaxLength(100, ErrorMessage = "City must not exceed 100 characters.")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Country is required.")]
    [MaxLength(100, ErrorMessage = "Country must not exceed 100 characters.")]
    public string Country { get; set; } = string.Empty;
}
