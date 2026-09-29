using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.Models;

public class PaymentMethod
{
    public int PaymentMethodId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
