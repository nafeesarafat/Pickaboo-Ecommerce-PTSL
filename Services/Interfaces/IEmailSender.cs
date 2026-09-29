namespace Ecommerce.Api.Services;

/// <summary>
/// Outbound email abstraction. The authentication layer only depends on this interface, so a
/// real SMTP or third-party provider can be introduced later without touching the auth services.
/// </summary>
public interface IEmailSender
{
    Task SendPasswordResetEmailAsync(
        string email,
        string userName,
        string resetToken,
        CancellationToken cancellationToken = default);

    Task SendEmailVerificationAsync(
        string email,
        string userName,
        string verificationToken,
        CancellationToken cancellationToken = default);
}
