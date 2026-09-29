namespace Ecommerce.Api.DTOs.Order;

/// <summary>
/// Read-only projection of an order's status, returned by the Order Status
/// endpoint (API 26) and the Cancel Order endpoint (API 27).
/// </summary>
public class OrderStatusResponseDto
{
    public int OrderId { get; set; }

    public string OrderStatus { get; set; } = string.Empty;

    public DateTime UpdateDate { get; set; }
}
