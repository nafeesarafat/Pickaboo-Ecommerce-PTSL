using System.ComponentModel.DataAnnotations;
using Ecommerce.Api.Configurations;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Models;
using Ecommerce.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ecommerce.Api.Data;

public class AdminSeeder
{
    private const int AdminRoleId = 1;
    private const string AdminRole = "Admin";

    private readonly AppDbContext _context;
    private readonly AdminSeedOptions _options;
    private readonly IPasswordHasher _passwordHasher;

    public AdminSeeder(
        AppDbContext context,
        IOptions<AdminSeedOptions> options,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _options = options.Value;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        bool adminExists = await _context.Administrators
            .AnyAsync(administrator => administrator.RoleId == AdminRoleId, cancellationToken)
            || await _context.UserInformation
                .AnyAsync(user => user.Role == AdminRole, cancellationToken);

        if (adminExists)
        {
            return;
        }

        var username = _options.Username?.Trim() ?? string.Empty;
        var email = _options.Email?.Trim() ?? string.Empty;
        var password = _options.Password;
        ValidateConfiguration(username, email, password);

        bool credentialsInUse = await _context.Administrators.AnyAsync(
                administrator => administrator.Username == username || administrator.Email == email,
                cancellationToken)
            || await _context.UserInformation.AnyAsync(
                user => user.UserName == username || user.Email == email,
                cancellationToken);

        if (credentialsInUse)
        {
            throw new InvalidOperationException(
                "The configured initial Admin username or email is already in use by a non-Admin account.");
        }

        var passwordHash = _passwordHasher.HashPassword(password);
        var now = DateTime.UtcNow;
        _context.Administrators.Add(new Administrator
        {
            FullName = username,
            Email = email,
            Username = username,
            PasswordHash = passwordHash,
            RoleId = AdminRoleId,
            AccountStatus = "Active",
            CreatedDate = now
        });
        _context.UserInformation.Add(new UserInformation
        {
            UserName = username,
            Email = email,
            PasswordHash = passwordHash,
            RegistrationDate = now,
            IsActive = true,
            IsEmailVerified = true,
            Role = AdminRole
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateConfiguration(string username, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(username)
            || username.Length is < 3 or > 100
            || string.IsNullOrWhiteSpace(email)
            || email.Length > 256
            || !new EmailAddressAttribute().IsValid(email)
            || !PasswordPolicy.IsValid(password))
        {
            throw new InvalidOperationException(
                "Initial Admin seeding is required but AdminSeed configuration is missing or invalid. " +
                "Set AdminSeed:Username, AdminSeed:Email, and a password meeting the account password policy " +
                "through user secrets or environment variables (AdminSeed__Username, AdminSeed__Email, " +
                "AdminSeed__Password).");
        }
    }
}
