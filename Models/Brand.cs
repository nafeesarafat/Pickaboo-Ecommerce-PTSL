using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.Models;

public class Brand
{
    public int BrandId { get; set; }

    [Required]
    [MaxLength(100)]
    public string BrandName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime UpdatedDate { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
