using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Api.Models;

public class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    // Financial snapshot values calculated at checkout and never changed by later edits.
    public decimal TotalAmount { get; set; }
    public decimal SubTotal { get; set; }
    public decimal ShippingFee { get; set; }

    [Required]
    [MaxLength(500)]
    public string ShippingAddress { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Required]
    [MaxLength(50)]
    public string OrderStatus { get; set; } = "Pending";

    [MaxLength(50)]
    public string? DeliveryStatus { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserInformation User { get; set; } = null!;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
