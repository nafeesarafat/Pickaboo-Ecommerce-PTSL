using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.Models;

/// <summary>
/// Application user. Passwords are never stored in plain text; only PasswordHash is persisted.
/// </summary>
public class UserInformation
{
    public int UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public DateTime RegistrationDate { get; set; }

    public DateTime? LastLoginDateTime { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsEmailVerified { get; set; }

    [MaxLength(128)]
    public string? EmailVerificationTokenHash { get; set; }

    public DateTime? EmailVerificationTokenExpiresAt { get; set; }

    [MaxLength(2048)]
    public string? TwoFactorSecretProtected { get; set; }

    [MaxLength(2048)]
    public string? TwoFactorQrCodeUrlProtected { get; set; }

    public bool IsTwoFactorEnabled { get; set; }

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "Customer";

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public NotificationSettings? NotificationSettings { get; set; }
}
