using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.Models;

public class ShippingZone
{
    public int ZoneId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ZoneName { get; set; } = string.Empty;

    public decimal Charge { get; set; }

    public ICollection<DeliveryArea> DeliveryAreas { get; set; } = new List<DeliveryArea>();
}
