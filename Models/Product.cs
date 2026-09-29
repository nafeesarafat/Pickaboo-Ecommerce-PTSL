using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Api.Models;

public class Product
{
    public int ProductId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    // Money values are configured as decimal(18, 2) in ProductConfiguration.
    public decimal Price { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public int CategoryId { get; set; }

    public int StockQuantity { get; set; }

    public bool IsFeatured { get; set; }

    public int BrandId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;

    [ForeignKey(nameof(BrandId))]
    public Brand Brand { get; set; } = null!;

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<StockHistory> StockHistoryEntries { get; set; } = new List<StockHistory>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
