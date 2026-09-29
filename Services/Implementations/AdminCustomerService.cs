using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Mappings;
using Ecommerce.Api.Models;
using Ecommerce.Api.Repositories;

namespace Ecommerce.Api.Services;

public class AdminCustomerService : IAdminCustomerService
{
    private readonly IAdminCustomerRepository _repository;
    private readonly IPasswordHasher _passwordHasher;

    public AdminCustomerService(
        IAdminCustomerRepository repository,
        IPasswordHasher passwordHasher)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
    }

    public async Task<AdminCustomerDto> AddAsync(
        AddAdminCustomerRequestDto request,
        CancellationToken cancellationToken)
    {
        string userName = request.UserName.Trim();
        string email = request.Email.Trim();
        if (userName.Length == 0)
        {
            throw new BadRequestException("UserName is required.");
        }

        var passwordErrors = PasswordPolicy.Validate(request.Password);
        if (passwordErrors.Count > 0)
        {
            throw new BadRequestException("Password does not meet the password policy.", passwordErrors);
        }

        if (await _repository.UserNameExistsAsync(userName, cancellationToken))
        {
            throw new ConflictException("This username is already taken.");
        }

        if (await _repository.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var customer = new UserInformation
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
            Role = AuthService.DefaultRole
        };

        await _repository.AddAsync(customer, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return ToCustomerDto(customer);
    }

    public async Task<AdminCustomerStatusDto> DeleteAsync(int userId, CancellationToken cancellationToken)
    {
        var customer = await GetCustomerForUpdateAsync(userId, cancellationToken);
        customer.IsActive = false;
        await _repository.SaveChangesAsync(cancellationToken);
        return new AdminCustomerStatusDto { UserId = customer.UserId, IsActive = customer.IsActive };
    }

    public async Task<PagedResultDto<AdminCustomerOrderDto>> GetOrdersAsync(
        int userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        if (userId <= 0)
        {
            throw new BadRequestException("A valid customer id is required.");
        }

        if (!await _repository.CustomerExistsAsync(userId, cancellationToken))
        {
            throw new NotFoundException($"Customer with id {userId} was not found.");
        }

        return await _repository.GetOrdersAsync(userId, pageNumber, pageSize, cancellationToken);
    }

    public async Task<AdminCustomerAddressDto> GetAddressAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        return await _repository.GetAddressAsync(userId, cancellationToken)
            ?? throw new NotFoundException($"Customer with id {userId} was not found.");
    }

    public async Task<AdminCustomerStatusDto> UpdateStatusAsync(
        int userId,
        bool isActive,
        CancellationToken cancellationToken)
    {
        var customer = await GetCustomerForUpdateAsync(userId, cancellationToken);
        customer.IsActive = isActive;
        await _repository.SaveChangesAsync(cancellationToken);
        return new AdminCustomerStatusDto { UserId = customer.UserId, IsActive = customer.IsActive };
    }

    public Task<PagedResultDto<AdminCustomerDto>> SearchAsync(
        AdminCustomerSearchRequestDto request,
        CancellationToken cancellationToken)
    {
        return _repository.SearchAsync(request, cancellationToken);
    }

    private async Task<UserInformation> GetCustomerForUpdateAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        if (userId <= 0)
        {
            throw new BadRequestException("A valid customer id is required.");
        }

        return await _repository.GetCustomerForUpdateAsync(userId, cancellationToken)
            ?? throw new NotFoundException($"Customer with id {userId} was not found.");
    }

    private static AdminCustomerDto ToCustomerDto(UserInformation user)
    {
        return new AdminCustomerDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            City = user.City,
            Country = user.Country,
            IsActive = user.IsActive,
            RegistrationDate = user.RegistrationDate
        };
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
