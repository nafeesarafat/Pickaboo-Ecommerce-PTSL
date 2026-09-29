using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Api.Models;

/// <summary>
/// Server-side record of a password reset request.
/// Only the SHA-256 hash of the reset token is stored - the raw token exists solely inside the
/// email that is sent to the account owner and is never persisted.
/// </summary>
public class PasswordResetToken
{
    public int PasswordResetTokenId { get; set; }

    public int UserId { get; set; }

    /// <summary>Base64 encoded SHA-256 hash of the raw reset token.</summary>
    [Required]
    [MaxLength(128)]
    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    /// <summary>Set when the token is consumed or superseded; a used token is never valid again.</summary>
    public DateTime? UsedAt { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserInformation User { get; set; } = null!;
}
