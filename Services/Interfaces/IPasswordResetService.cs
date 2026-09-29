using Ecommerce.Api.Models;

namespace Ecommerce.Api.Services;

/// <summary>
/// Issues password reset tokens: creates a cryptographically random token, stores only its hash
/// with an expiry, invalidates any earlier unused token for the same account, and hands the raw
/// token to <see cref="IEmailSender"/> for delivery.
/// </summary>
public interface IPasswordResetService
{
    /// <summary>
    /// Creates a fresh reset token for the given user and sends it. Returns the raw token so the
    /// caller can forward it; the raw value is never persisted.
    /// </summary>
    Task<string> IssueResetTokenAsync(UserInformation user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and consumes a reset token while updating the associated user's password.
    /// </summary>
    Task ResetPasswordAsync(
        string rawToken,
        string newPassword,
        string confirmPassword,
        CancellationToken cancellationToken = default);
}
