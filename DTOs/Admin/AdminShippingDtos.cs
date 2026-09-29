using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Admin;

public class AdminShippingMethodDto
{
    public int ShippingMethodId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class AdminShippingZoneDto
{
    public int ZoneId { get; set; }
    public string ZoneName { get; set; } = string.Empty;
}

public class AdminShippingChargeDto
{
    public int ZoneId { get; set; }
    public string ZoneName { get; set; } = string.Empty;
    public decimal Charge { get; set; }
}

public class AdminDeliveryAreaDto
{
    public int AreaId { get; set; }
    public string AreaName { get; set; } = string.Empty;
    public int ZoneId { get; set; }
    public string ZoneName { get; set; } = string.Empty;
}

public class UpdateDeliveryStatusRequestDto
{
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;

    public DateTime UpdatedDate { get; set; }
}

public class AdminDeliveryStatusDto
{
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedDate { get; set; }
}
