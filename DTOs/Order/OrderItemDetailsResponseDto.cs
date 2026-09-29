namespace Ecommerce.Api.DTOs.Order;

public class OrderItemDetailsResponseDto
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public decimal TotalAmount { get; set; }
}