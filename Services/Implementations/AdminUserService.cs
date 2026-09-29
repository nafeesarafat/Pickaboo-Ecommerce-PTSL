using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Services;

public class AdminUserService : IAdminUserService
{
    private readonly AppDbContext _context;

    public AdminUserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AdminUserDto>> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        return await _context.UserInformation
            .AsNoTracking()
            .OrderBy(user => user.UserId)
            .Select(user => new AdminUserDto
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RegistrationDate = user.RegistrationDate,
                LastLoginDateTime = user.LastLoginDateTime,
                IsActive = user.IsActive,
                RoleId = user.Role
            })
            .ToArrayAsync(cancellationToken);
    }

    public async Task<AdminUserDetailsDto> GetUserDetailsAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        var user = await _context.UserInformation
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .Select(item => new AdminUserDetailsDto
            {
                UserId = item.UserId,
                UserName = item.UserName,
                Email = item.Email,
                Address = item.Address,
                City = item.City,
                Country = item.Country,
                PhoneNumber = item.PhoneNumber,
                RoleName = item.Role,
                TotalOrders = item.Orders.Count,
                RegistrationDate = item.RegistrationDate
            })
            .SingleOrDefaultAsync(cancellationToken);

        return user ?? throw new NotFoundException($"User with id {userId} was not found.");
    }

    public async Task<AdminUserDto> UpdateUserAsync(
        int userId,
        UpdateAdminUserRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await _context.UserInformation
            .SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken)
            ?? throw new NotFoundException($"User with id {userId} was not found.");

        var userName = request.UserName.Trim();
        var email = request.Email.Trim();
        var role = request.RoleId.Trim();

        if (await _context.UserInformation.AnyAsync(
                item => item.UserId != userId && item.UserName == userName,
                cancellationToken))
        {
            throw new ConflictException("This username is already taken.");
        }

        if (await _context.UserInformation.AnyAsync(
                item => item.UserId != userId && item.Email == email,
                cancellationToken))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        bool roleExists = await _context.UserInformation
            .AnyAsync(item => item.Role == role, cancellationToken);
        if (!roleExists)
        {
            throw new BadRequestException("The specified role does not exist.");
        }

        user.UserName = userName;
        user.Email = email;
        user.Address = NullIfWhiteSpace(request.Address);
        user.City = NullIfWhiteSpace(request.City);
        user.Country = NullIfWhiteSpace(request.Country);
        user.PhoneNumber = NullIfWhiteSpace(request.PhoneNumber);
        user.IsActive = request.IsActive;
        user.Role = role;

        _context.ActivityLogs.Add(new ActivityLog
        {
            UserId = user.UserId,
            Action = "Updated by administrator",
            Description = "User account information was updated by an administrator.",
            Timestamp = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(cancellationToken);
        return ToAdminUserDto(user);
    }

    private static AdminUserDto ToAdminUserDto(UserInformation user)
    {
        return new AdminUserDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            RegistrationDate = user.RegistrationDate,
            LastLoginDateTime = user.LastLoginDateTime,
            IsActive = user.IsActive,
            RoleId = user.Role
        };
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
