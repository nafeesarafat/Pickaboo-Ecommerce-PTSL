namespace Ecommerce.Api.DTOs.Order;

/// <summary>
/// Detailed read-only projection of a single order, returned by the Order Details
/// endpoint (API 25). Includes the unit price of the purchased product(s).
/// </summary>
public class OrderDetailsResponseDto
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public string ShippingAddress { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string OrderStatus { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    /// <summary>Unit price of the ordered product, snapshotted from the database at purchase time.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Total amount charged for the order (subtotal + shipping fee).</summary>
    public decimal TotalAmount { get; set; }

    public decimal SubTotal { get; set; }

    public decimal ShippingFee { get; set; }
}
