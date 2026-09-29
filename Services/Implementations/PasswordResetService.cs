using System.Security.Cryptography;
using System.Text;
using Ecommerce.Api.Configurations;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Data;
using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ecommerce.Api.Services;

public class PasswordResetService : IPasswordResetService
{
    private const int TokenSizeInBytes = 32;

    private readonly AppDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly IPasswordHasher _passwordHasher;
    private readonly PasswordResetOptions _options;
    private readonly ILogger<PasswordResetService> _logger;

    public PasswordResetService(
        AppDbContext context,
        IEmailSender emailSender,
        IPasswordHasher passwordHasher,
        IOptions<PasswordResetOptions> options,
        ILogger<PasswordResetService> logger)
    {
        _context = context;
        _emailSender = emailSender;
        _passwordHasher = passwordHasher;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> IssueResetTokenAsync(UserInformation user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        // Supersede any token that is still outstanding, so only the newest link ever works.
        DateTime now = DateTime.UtcNow;
        var superseded = await _context.PasswordResetTokens
            .Where(t => t.UserId == user.UserId && t.UsedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in superseded)
        {
            token.UsedAt = now;
        }

        string rawToken = GenerateRawToken();

        _context.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.UserId,
            TokenHash = HashToken(rawToken),
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(_options.ExpiryMinutes)
        });

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Password reset token issued for user {UserId}, valid until {ExpiresAt}.",
            user.UserId,
            now.AddMinutes(_options.ExpiryMinutes));

        await _emailSender.SendPasswordResetEmailAsync(user.Email, user.UserName, rawToken, cancellationToken);

        return rawToken;
    }

    public async Task ResetPasswordAsync(
        string rawToken,
        string newPassword,
        string confirmPassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            throw InvalidOrExpiredToken();
        }

        List<string> passwordFailures = PasswordPolicy.Validate(newPassword);
        if (passwordFailures.Count > 0)
        {
            throw new BadRequestException("Password does not meet the requirements.", passwordFailures);
        }

        if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
        {
            throw new BadRequestException("NewPassword and ConfirmPassword do not match.");
        }

        string tokenHash = HashToken(rawToken);
        PasswordResetToken? resetToken = await _context.PasswordResetTokens
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        DateTime now = DateTime.UtcNow;
        if (resetToken is null || resetToken.UsedAt is not null || resetToken.ExpiresAt <= now)
        {
            throw InvalidOrExpiredToken();
        }

        UserInformation? user = await _context.UserInformation
            .SingleOrDefaultAsync(candidate => candidate.UserId == resetToken.UserId, cancellationToken);
        if (user is null)
        {
            throw InvalidOrExpiredToken();
        }

        string passwordHash = _passwordHasher.HashPassword(newPassword);
        var executionStrategy = _context.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            DateTime consumedAt = DateTime.UtcNow;

            int consumedTokenCount = await _context.PasswordResetTokens
                .Where(token =>
                    token.PasswordResetTokenId == resetToken.PasswordResetTokenId
                    && token.UsedAt == null
                    && token.ExpiresAt > consumedAt)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(token => token.UsedAt, consumedAt),
                    cancellationToken);

            if (consumedTokenCount != 1)
            {
                throw InvalidOrExpiredToken();
            }

            int updatedUserCount = await _context.UserInformation
                .Where(candidate => candidate.UserId == user.UserId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(candidate => candidate.PasswordHash, passwordHash),
                    cancellationToken);

            if (updatedUserCount != 1)
            {
                throw InvalidOrExpiredToken();
            }

            await transaction.CommitAsync(cancellationToken);
        });
    }

    private static BadRequestException InvalidOrExpiredToken() =>
        new("Invalid or expired password reset token.");

    private static string GenerateRawToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(TokenSizeInBytes);
        return Base64UrlEncode(bytes);
    }

    /// <summary>
    /// A plain SHA-256 is safe here because the token is a full 256-bit random value, not a
    /// user-chosen password: there is nothing to brute force, and the digest is not reversible.
    /// </summary>
    private static string HashToken(string rawToken)
    {
        byte[] digest = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToBase64String(digest);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
}
