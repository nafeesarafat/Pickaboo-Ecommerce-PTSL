using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Api.Models;

public class DeliveryArea
{
    public int AreaId { get; set; }

    [Required]
    [MaxLength(100)]
    public string AreaName { get; set; } = string.Empty;

    public int ZoneId { get; set; }

    [ForeignKey(nameof(ZoneId))]
    public ShippingZone Zone { get; set; } = null!;
}
