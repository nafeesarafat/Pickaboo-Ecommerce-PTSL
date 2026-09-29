using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Admin;

public class UpdateProductRequestDto
{
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    public int ProductId { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue)]
    public int BrandId { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Price { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsActive { get; set; }

    public DateTime UpdatedDate { get; set; }
}