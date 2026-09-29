namespace Ecommerce.Api.DTOs.Admin;

public class AdminOrderStatusDto
{
    public int OrderId { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public DateTime UpdatedDate { get; set; }
}
