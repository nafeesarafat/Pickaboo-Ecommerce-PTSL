using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Admin;

public class PaymentMethodRequestDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class AdminPaymentMethodDto
{
    public int PaymentMethodId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
