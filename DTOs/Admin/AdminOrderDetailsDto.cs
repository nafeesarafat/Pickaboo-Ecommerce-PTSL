namespace Ecommerce.Api.DTOs.Admin;

public class AdminOrderDetailsDto
{
    public int OrderId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal FinalAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public IReadOnlyList<AdminOrderItemDto> OrderItems { get; set; } = Array.Empty<AdminOrderItemDto>();
}

public class AdminOrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
    public int Quantity { get; set; }
}
