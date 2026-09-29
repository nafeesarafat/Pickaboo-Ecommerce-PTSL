namespace Ecommerce.Api.DTOs.Order;

/// <summary>
/// Read-only projection of a single order, returned by the My Orders (API 24) and
/// Place Order (API 23) endpoints. All monetary values are server-calculated snapshots.
/// </summary>
public class OrderResponseDto
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

    public decimal SubTotal { get; set; }

    public decimal ShippingFee { get; set; }

    public decimal FinalAmount { get; set; }
}
