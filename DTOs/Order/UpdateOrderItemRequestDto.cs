using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Order;

public class UpdateOrderItemRequestDto
{
    [Range(1, int.MaxValue)]
    public int StockQuantity { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
}