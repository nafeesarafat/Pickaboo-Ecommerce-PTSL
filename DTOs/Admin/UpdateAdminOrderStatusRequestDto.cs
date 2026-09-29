using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Admin;

public class UpdateAdminOrderStatusRequestDto
{
    [Required]
    [MaxLength(50)]
    public string OrderStatus { get; set; } = string.Empty;
}
