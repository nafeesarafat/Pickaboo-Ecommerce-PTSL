using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Admin;

public class AdjustStockRequestDto
{
    public int Change { get; set; }

    [Required]
    [MaxLength(255)]
    public string Reason { get; set; } = string.Empty;
}
