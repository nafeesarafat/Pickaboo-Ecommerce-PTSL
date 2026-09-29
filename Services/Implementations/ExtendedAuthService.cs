using System.Security.Cryptography;
using System.Text;
using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs.Auth;
using Ecommerce.Api.Helpers;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Ecommerce.Api.Services;

public class ExtendedAuthService : IExtendedAuthService
{
    private const string TwoFactorProtectionPurpose = "Ecommerce.Api.UserInformation.TwoFactorSecret.v1";
    private const string TwoFactorQrProtectionPurpose = "Ecommerce.Api.UserInformation.TwoFactorQrCodeUrl.v1";
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDataProtector _secretProtector;
    private readonly IDataProtector _qrUrlProtector;

    public ExtendedAuthService(
        AppDbContext context,
        ICurrentUserService currentUser,
        IDataProtectionProvider protectionProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _secretProtector = protectionProvider.CreateProtector(TwoFactorProtectionPurpose);
        _qrUrlProtector = protectionProvider.CreateProtector(TwoFactorQrProtectionPurpose);
    }

    public async Task<VerifyEmailResultDto> VerifyEmailAsync(
        VerifyEmailRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await _context.UserInformation
            .SingleOrDefaultAsync(item => item.UserId == request.UserId, cancellationToken)
            ?? throw new NotFoundException("The account associated with this verification token was not found.");

        if (user.IsEmailVerified)
        {
            throw new BadRequestException("The email verification token is invalid or has expired.");
        }

        if (user.EmailVerificationTokenHash is null
            || !user.EmailVerificationTokenExpiresAt.HasValue
            || user.EmailVerificationTokenExpiresAt.Value <= DateTime.UtcNow)
        {
            throw new BadRequestException("The email verification token is invalid or has expired.");
        }

        byte[] expectedHash = Convert.FromBase64String(user.EmailVerificationTokenHash);
        byte[] actualHash = SHA256.HashData(Encoding.UTF8.GetBytes(request.Token));
        if (!CryptographicOperations.FixedTimeEquals(expectedHash, actualHash))
        {
            throw new BadRequestException("The email verification token is invalid or has expired.");
        }

        user.IsEmailVerified = true;
        user.EmailVerificationTokenHash = null;
        user.EmailVerificationTokenExpiresAt = null;
        await _context.SaveChangesAsync(cancellationToken);
        return new VerifyEmailResultDto { UserId = user.UserId, IsVerified = true };
    }

    public async Task<TwoFactorStatusDto> UpdateTwoFactorAsync(
        EnableTwoFactorRequestDto request,
        CancellationToken cancellationToken)
    {
        int userId = _currentUser.GetRequiredUserId();
        if (request.UserId != userId)
        {
            throw new ForbiddenException("The user id must match the authenticated account.");
        }

        var user = await _context.UserInformation
            .SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("The authenticated account was not found.");
        string secret = request.Secret.Trim();
        if (request.IsEnabled && !Regex.IsMatch(secret, "^[A-Z2-7]{16,128}$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            throw new BadRequestException("Secret must be a valid Base32 authenticator secret.");
        }

        user.IsTwoFactorEnabled = request.IsEnabled;
        user.TwoFactorSecretProtected = request.IsEnabled
            ? _secretProtector.Protect(secret.ToUpperInvariant())
            : null;
        user.TwoFactorQrCodeUrlProtected = request.IsEnabled && !string.IsNullOrWhiteSpace(request.QrCodeUrl)
            ? _qrUrlProtector.Protect(request.QrCodeUrl.Trim())
            : null;
        await _context.SaveChangesAsync(cancellationToken);
        return new TwoFactorStatusDto { IsEnabled = user.IsTwoFactorEnabled };
    }

    public async Task<IReadOnlyList<UserSessionDto>> GetSessionsAsync(int userId, CancellationToken cancellationToken)
    {
        return await _context.Sessions.AsNoTracking()
            .Where(session => session.UserId == userId && session.IsActive && session.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(session => session.LoginDateTime)
            .Select(session => new UserSessionDto
            {
                SessionId = session.SessionId.ToString(),
                UserId = session.UserId,
                Device = session.Device,
                LoginDateTime = session.LoginDateTime
            })
            .ToArrayAsync(cancellationToken);
    }

    public async Task EndSessionAsync(int userId, int? sessionId, CancellationToken cancellationToken)
    {
        if (!sessionId.HasValue)
        {
            return;
        }

        var session = await _context.Sessions.SingleOrDefaultAsync(
            item => item.SessionId == sessionId.Value && item.UserId == userId,
            cancellationToken);
        if (session is not null && session.IsActive)
        {
            session.IsActive = false;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<LoginActivityDto>> GetLoginActivityAsync(CancellationToken cancellationToken)
    {
        return await _context.LoginLogs.AsNoTracking()
            .OrderByDescending(log => log.LoginDateTime)
            .Select(log => new LoginActivityDto
            {
                AdministratorId = log.AdministratorId,
                AdministratorName = log.AdministratorName,
                LoginDateTime = log.LoginDateTime,
                IpAddress = log.IpAddress
            })
            .ToArrayAsync(cancellationToken);
    }
}
