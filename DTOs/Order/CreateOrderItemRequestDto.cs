using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Order;

public class CreateOrderItemRequestDto
{
    public int OrderId { get; set; }
    public int UserId { get; set; }

    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    public string? ProductName { get; set; }
    public decimal UnitPrice { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public decimal TotalAmount { get; set; }
}