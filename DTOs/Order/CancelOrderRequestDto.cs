namespace Ecommerce.Api.DTOs.Order;

/// <summary>
/// Request body for cancelling an order (API 27). Every field in this DTO is
/// client-supplied and ignored by the server: UserId comes from the JWT, the
/// OrderId comes from the route, and the status / cancellation date are set
/// server-side according to the cancellation business rule.
/// </summary>
public class CancelOrderRequestDto
{
    /// <summary>Ignored — the OrderId is taken from the route.</summary>
    public int OrderId { get; set; }

    /// <summary>Ignored — the server sets the status to "Cancelled".</summary>
    public int OrderStatus { get; set; }

    /// <summary>Ignored — the UserId is taken from the JWT.</summary>
    public int UserId { get; set; }

    /// <summary>Ignored — the server sets the cancellation date to the current time.</summary>
    public DateTime CancelledDate { get; set; }
}
