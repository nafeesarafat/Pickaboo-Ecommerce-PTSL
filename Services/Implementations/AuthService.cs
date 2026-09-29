using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs.Auth;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Ecommerce.Api.Services;

public class AuthService : IAuthService
{
    /// <summary>Role assigned to every account created through the public registration API.</summary>
    public const string DefaultRole = "Customer";

    /// <summary>
    /// A throwaway BCrypt hash used to spend roughly the same amount of time on "user does not
    /// exist" as on a real password check, so response timing does not disclose whether an
    /// email address is registered.
    /// </summary>
    private static readonly Lazy<string> DecoyHash = new(() =>
        new PasswordHasher().HashPassword(Guid.NewGuid().ToString()));

    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IPasswordResetService _passwordResetService;
    private readonly IEmailSender _emailSender;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IPasswordResetService passwordResetService,
        IEmailSender emailSender,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _passwordResetService = passwordResetService;
        _emailSender = emailSender;
        _httpContextAccessor = httpContextAccessor;
    }

    // ------------------------------------------------------------------ API 1
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        string email = request.Email.Trim();

        var user = await _context.UserInformation
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null)
        {
            // Spend comparable time so a missing account is not measurably faster than a wrong password.
            _passwordHasher.VerifyPassword(request.Password, DecoyHash.Value);

            throw new AuthenticationFailedException("Invalid email or password.");
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            await RecordFailedAdminLoginAsync(user, cancellationToken);
            throw new AuthenticationFailedException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            await RecordFailedAdminLoginAsync(user, cancellationToken);
            throw new ForbiddenException("This account has been deactivated. Please contact support.");
        }

        DateTime loginDateTime = DateTime.UtcNow;
        user.LastLoginDateTime = loginDateTime;
        var session = new Session
        {
            UserId = user.UserId,
            Device = Truncate(_httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString(), 200),
            LoginDateTime = loginDateTime,
            IsActive = true
        };
        _context.Sessions.Add(session);

        if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            var administrator = await _context.Administrators
                .SingleOrDefaultAsync(item => item.Email == user.Email, cancellationToken);
            if (administrator is not null)
            {
                _context.LoginLogs.Add(new LoginLog
                {
                    AdministratorId = administrator.AdministratorId,
                    AdministratorName = administrator.FullName,
                    LoginDateTime = loginDateTime,
                    IpAddress = Truncate(_httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(), 45),
                    Status = "Success"
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        var token = _jwtService.GenerateAccessToken(user, session.SessionId);
        session.ExpiresAtUtc = token.ExpiresAtUtc;
        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto
        {
            Token = token.Token,
            TokenType = "Bearer",
            ExpiresAtUtc = token.ExpiresAtUtc,
            ExpiresInMinutes = (int)Math.Round((token.ExpiresAtUtc - DateTime.UtcNow).TotalMinutes),
            User = MapToProfile(user)
        };
    }

    // ------------------------------------------------------------------ API 2
    public async Task<RegisterResponseDto> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        string email = request.Email.Trim();
        string userName = request.UserName.Trim();

        bool emailTaken = await _context.UserInformation
            .AnyAsync(u => u.Email == email, cancellationToken);

        if (emailTaken)
        {
            throw new ConflictException("An account with this email already exists.");
        }

        bool userNameTaken = await _context.UserInformation
            .AnyAsync(u => u.UserName == userName, cancellationToken);

        if (userNameTaken)
        {
            throw new ConflictException("This username is already taken.");
        }

        // PasswordHash, Role, IsActive, RegistrationDate and UserId are all server controlled.
        // They are never read from the request.
        var user = new UserInformation
        {
            UserName = userName,
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            PhoneNumber = NullIfWhiteSpace(request.PhoneNumber),
            Address = NullIfWhiteSpace(request.Address),
            City = NullIfWhiteSpace(request.City),
            Country = NullIfWhiteSpace(request.Country),
            RegistrationDate = DateTime.UtcNow,
            IsActive = true,
            Role = DefaultRole
        };

        string verificationToken = GenerateVerificationToken();
        user.EmailVerificationTokenHash = HashVerificationToken(verificationToken);
        user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
        _context.UserInformation.Add(user);
        _context.ActivityLogs.Add(new ActivityLog
        {
            User = user,
            Action = "Registered",
            Description = "Customer account was registered.",
            Timestamp = user.RegistrationDate
        });
        await _context.SaveChangesAsync(cancellationToken);
        await _emailSender.SendEmailVerificationAsync(user.Email, user.UserName, verificationToken, cancellationToken);

        return new RegisterResponseDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role,
            RegistrationDate = user.RegistrationDate
        };
    }

    // ------------------------------------------------------------------ API 4
    public async Task<ProfileResponseDto> GetProfileAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _context.UserInformation
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("User profile not found.");

        return MapToProfile(user);
    }

    // ------------------------------------------------------------------ API 5
    public async Task<ProfileResponseDto> UpdateProfileAsync(
        int userId,
        UpdateProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await _context.UserInformation
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("User profile not found.");

        string email = request.Email.Trim();
        string? verificationToken = null;

        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            bool emailTaken = await _context.UserInformation
                .AnyAsync(u => u.Email == email && u.UserId != userId, cancellationToken);

            if (emailTaken)
            {
                throw new ConflictException("An account with this email already exists.");
            }

            user.IsEmailVerified = false;
            verificationToken = GenerateVerificationToken();
            user.EmailVerificationTokenHash = HashVerificationToken(verificationToken);
            user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
        }

        // Only the fields the roadmap allows to be edited are touched here.
        user.UserName = request.UserName.Trim();
        user.Email = email;
        user.PhoneNumber = NullIfWhiteSpace(request.PhoneNumber);
        user.Address = NullIfWhiteSpace(request.Address);
        user.City = NullIfWhiteSpace(request.City);
        user.Country = NullIfWhiteSpace(request.Country);

        _context.ActivityLogs.Add(new ActivityLog
        {
            UserId = user.UserId,
            Action = "Profile updated",
            Description = "User profile information was updated.",
            Timestamp = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(cancellationToken);
        if (verificationToken is not null)
        {
            await _emailSender.SendEmailVerificationAsync(user.Email, user.UserName, verificationToken, cancellationToken);
        }

        return MapToProfile(user);
    }

    // ------------------------------------------------------------------ API 6
    public async Task ChangePasswordAsync(
        int userId,
        ChangePasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await _context.UserInformation
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("User profile not found.");

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new BadRequestException("The current password is incorrect.");
        }

        if (_passwordHasher.VerifyPassword(request.NewPassword, user.PasswordHash))
        {
            throw new BadRequestException("The new password must be different from the current password.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);

        await _context.SaveChangesAsync(cancellationToken);
    }

    // ------------------------------------------------------------------ API 7
    public async Task RequestPasswordResetAsync(
        ForgotPasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        string email = request.Email.Trim();

        var user = await _context.UserInformation
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null)
        {
            // Identical response and comparable work, so a caller cannot tell registered emails apart.
            _passwordHasher.VerifyPassword(Guid.NewGuid().ToString(), DecoyHash.Value);
            return;
        }

        if (!user.IsActive)
        {
            return;
        }

        await _passwordResetService.IssueResetTokenAsync(user, cancellationToken);
    }

    private static ProfileResponseDto MapToProfile(UserInformation user) => new()
    {
        UserId = user.UserId,
        UserName = user.UserName,
        Email = user.Email,
        Address = user.Address,
        City = user.City,
        Country = user.Country,
        PhoneNumber = user.PhoneNumber,
        RegistrationDate = user.RegistrationDate,
        IsActive = user.IsActive,
        Role = user.Role
    };

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string GenerateVerificationToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static string HashVerificationToken(string token) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static string? Truncate(string? value, int maxLength) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Length <= maxLength ? value : value[..maxLength];

    private async Task RecordFailedAdminLoginAsync(UserInformation user, CancellationToken cancellationToken)
    {
        if (!string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var administrator = await _context.Administrators
            .SingleOrDefaultAsync(item => item.Email == user.Email, cancellationToken);
        if (administrator is null)
        {
            return;
        }

        _context.LoginLogs.Add(new LoginLog
        {
            AdministratorId = administrator.AdministratorId,
            AdministratorName = administrator.FullName,
            LoginDateTime = DateTime.UtcNow,
            IpAddress = Truncate(_httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(), 45),
            Status = "Failed"
        });
        await _context.SaveChangesAsync(cancellationToken);
    }
}
