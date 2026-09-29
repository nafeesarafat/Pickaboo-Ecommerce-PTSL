using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Admin;

public class AddCategoryRequestDto
{
    [Required]
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;
}
