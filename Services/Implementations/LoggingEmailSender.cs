using Ecommerce.Api.Configurations;
using Microsoft.Extensions.Options;

namespace Ecommerce.Api.Services;

/// <summary>
/// Development stand-in for a real mail provider.
/// The project has no SMTP infrastructure yet, so the reset link is written to the application log
/// instead of being emailed. It is emitted only outside production: in production the token is never
/// written anywhere, so nothing leaks while a real <see cref="IEmailSender"/> is still pending.
/// </summary>
public class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;
    private readonly IHostEnvironment _environment;
    private readonly PasswordResetOptions _options;

    public LoggingEmailSender(
        ILogger<LoggingEmailSender> logger,
        IHostEnvironment environment,
        IOptions<PasswordResetOptions> options)
    {
        _logger = logger;
        _environment = environment;
        _options = options.Value;
    }

    public Task SendPasswordResetEmailAsync(
        string email,
        string userName,
        string resetToken,
        CancellationToken cancellationToken = default)
    {
        if (_environment.IsProduction())
        {
            // Never expose a reset secret in production until a real email provider is configured.
            _logger.LogWarning(
                "A password reset was requested for account {UserId} but no email provider is configured. " +
                "The reset link was discarded.",
                userName);
            return Task.CompletedTask;
        }

        string link = string.IsNullOrWhiteSpace(_options.ResetUrlBase)
            ? resetToken
            : $"{_options.ResetUrlBase.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(resetToken)}";

        _logger.LogWarning(
            "[DEVELOPMENT ONLY] Password reset link for {Email}: {ResetLink}. " +
            "Replace LoggingEmailSender with a real provider before deploying.",
            email,
            link);

        return Task.CompletedTask;
    }

    public Task SendEmailVerificationAsync(
        string email,
        string userName,
        string verificationToken,
        CancellationToken cancellationToken = default)
    {
        if (_environment.IsProduction())
        {
            _logger.LogWarning(
                "Email verification was requested for account {UserName} but no email provider is configured. " +
                "The verification token was discarded.",
                userName);
            return Task.CompletedTask;
        }

        string link = string.IsNullOrWhiteSpace(_options.ResetUrlBase)
            ? verificationToken
            : $"{_options.ResetUrlBase.TrimEnd('/')}/verify-email?token={Uri.EscapeDataString(verificationToken)}";

        _logger.LogWarning(
            "[DEVELOPMENT ONLY] Email verification link for {Email}: {VerificationLink}. " +
            "Replace LoggingEmailSender with a real provider before deploying.",
            email,
            link);
        return Task.CompletedTask;
    }
}
