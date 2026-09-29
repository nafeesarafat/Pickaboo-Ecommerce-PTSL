using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Admin;

public class AddProductRequestDto
{
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    public bool IsFeatured { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Price { get; set; }

    public string? CategoryName { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [Range(1, int.MaxValue)]
    public int BrandId { get; set; }

    public string? BrandName { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; }
}