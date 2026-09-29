using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Api.Models;

public class Session
{
    public int SessionId { get; set; }

    public int UserId { get; set; }

    [MaxLength(200)]
    public string? Device { get; set; }

    public DateTime LoginDateTime { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(UserId))]
    public UserInformation User { get; set; } = null!;
}
