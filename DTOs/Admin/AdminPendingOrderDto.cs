namespace Ecommerce.Api.DTOs.Admin;

public class AdminPendingOrderDto
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
}
