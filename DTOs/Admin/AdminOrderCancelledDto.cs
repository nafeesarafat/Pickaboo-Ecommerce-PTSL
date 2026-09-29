namespace Ecommerce.Api.DTOs.Admin;

public class AdminOrderCancelledDto
{
    public int OrderId { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public DateTime CancelledDate { get; set; }
}
