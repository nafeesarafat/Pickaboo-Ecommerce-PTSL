using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.Models;

public class ShippingMethod
{
    public int ShippingMethodId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
