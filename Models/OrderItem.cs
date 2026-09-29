using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Api.Models;

/// <summary>
/// Line item of an order.
/// Price and TotalAmount are copied from the product at the moment of purchase so that
/// historical order values never change when the catalog price is edited later.
/// </summary>
public class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public decimal TotalAmount { get; set; }

    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = null!;
}
