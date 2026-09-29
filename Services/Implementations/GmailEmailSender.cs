using System.Net.Sockets;
using System.Text.Encodings.Web;
using Ecommerce.Api.Configurations;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Ecommerce.Api.Services;

public class GmailEmailSender : IEmailSender
{
    private readonly EmailOptions _emailOptions;
    private readonly PasswordResetOptions _passwordResetOptions;
    private readonly ILogger<GmailEmailSender> _logger;

    public GmailEmailSender(
        IOptions<EmailOptions> emailOptions,
        IOptions<PasswordResetOptions> passwordResetOptions,
        ILogger<GmailEmailSender> logger)
    {
        _emailOptions = emailOptions.Value;
        _passwordResetOptions = passwordResetOptions.Value;
        _logger = logger;
    }

    public Task SendPasswordResetEmailAsync(
        string email,
        string userName,
        string resetToken,
        CancellationToken cancellationToken = default)
    {
        string resetUrl =
            $"{_passwordResetOptions.ResetUrlBase.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(resetToken)}";
        string safeUserName = HtmlEncoder.Default.Encode(userName);
        string safeResetUrl = HtmlEncoder.Default.Encode(resetUrl);

        string htmlBody = $"""
            <!doctype html>
            <html lang="en">
            <body style="margin:0;padding:0;background-color:#f4f6f8;font-family:Arial,sans-serif;color:#202124;">
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="padding:32px 12px;background-color:#f4f6f8;">
                <tr><td align="center">
                  <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:560px;background:#ffffff;border-radius:12px;padding:36px;">
                    <tr><td>
                      <h1 style="margin:0 0 24px;color:#202124;font-size:26px;">Pickaboo</h1>
                      <p style="font-size:16px;">Hello {safeUserName},</p>
                      <p style="font-size:16px;line-height:1.6;">We received a request to reset your Pickaboo password. Use the button below to choose a new password.</p>
                      <p style="margin:28px 0;">
                        <a href="{safeResetUrl}" style="display:inline-block;padding:14px 24px;background:#e53935;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:bold;">Reset Password</a>
                      </p>
                      <p style="font-size:14px;line-height:1.6;">If the button does not work, copy and paste this URL into your browser:<br><a href="{safeResetUrl}" style="color:#1565c0;word-break:break-all;">{safeResetUrl}</a></p>
                      <p style="font-size:14px;line-height:1.6;">This link expires after {_passwordResetOptions.ExpiryMinutes} minutes.</p>
                      <p style="font-size:14px;line-height:1.6;">If you did not request a password reset, please ignore this email. Your password will not change.</p>
                      <p style="margin-top:32px;color:#5f6368;font-size:13px;">The Pickaboo team</p>
                    </td></tr>
                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """;

        return SendMessageAsync(
            email,
            "Reset your Pickaboo password",
            htmlBody,
            "password reset",
            cancellationToken);
    }

    public Task SendEmailVerificationAsync(
        string email,
        string userName,
        string verificationToken,
        CancellationToken cancellationToken = default)
    {
        string verificationUrl =
            $"{_passwordResetOptions.ResetUrlBase.TrimEnd('/')}/verify-email?token={Uri.EscapeDataString(verificationToken)}";
        string safeUserName = HtmlEncoder.Default.Encode(userName);
        string safeVerificationUrl = HtmlEncoder.Default.Encode(verificationUrl);
        string htmlBody = $"""
            <!doctype html>
            <html lang="en">
            <body style="font-family:Arial,sans-serif;color:#202124;">
              <h1>Pickaboo</h1>
              <p>Hello {safeUserName},</p>
              <p>Please verify your email address by visiting this link:</p>
              <p><a href="{safeVerificationUrl}">Verify your email</a></p>
            </body>
            </html>
            """;

        return SendMessageAsync(
            email,
            "Verify your Pickaboo email address",
            htmlBody,
            "email verification",
            cancellationToken);
    }

    private async Task SendMessageAsync(
        string recipient,
        string subject,
        string htmlBody,
        string purpose,
        CancellationToken cancellationToken)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailOptions.FromName, _emailOptions.FromEmail));
        message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(
                _emailOptions.Host,
                _emailOptions.Port,
                _emailOptions.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto,
                cancellationToken);
            await client.AuthenticateAsync(
                _emailOptions.Username,
                _emailOptions.Password,
                cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is ProtocolException
                or ServiceNotConnectedException
                or ServiceNotAuthenticatedException
                or MailKit.Security.AuthenticationException
                or IOException
                or SocketException
                or System.Security.Authentication.AuthenticationException)
        {
            _logger.LogError(exception, "Failed to send {EmailPurpose} email through Gmail SMTP.", purpose);
        }
    }
}
