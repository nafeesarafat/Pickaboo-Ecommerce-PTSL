using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Api.Models;

public class Notification
{
    public int NotificationId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public int? OrderId { get; set; }

    public int? UserId { get; set; }

    public int? ProductId { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedDate { get; set; }

    [ForeignKey(nameof(OrderId))]
    public Order? Order { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserInformation? User { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }
}
