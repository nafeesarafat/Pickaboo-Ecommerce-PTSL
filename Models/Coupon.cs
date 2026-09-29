using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.Models;

public class Coupon
{
    public int CouponId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    public decimal DiscountAmount { get; set; }

    public int UsageCount { get; set; }

    public DateTime ExpiryDate { get; set; }
}
