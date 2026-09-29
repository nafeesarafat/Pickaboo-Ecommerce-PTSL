using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Mappings;
using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Repositories;

public class AdminCustomerRepository : IAdminCustomerRepository
{
    private readonly AppDbContext _context;

    public AdminCustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken)
    {
        return _context.UserInformation.AnyAsync(
            user => user.UserName == userName,
            cancellationToken);
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        return _context.UserInformation.AnyAsync(
            user => user.Email == email,
            cancellationToken);
    }

    public Task<bool> CustomerExistsAsync(int userId, CancellationToken cancellationToken)
    {
        return _context.UserInformation.AnyAsync(
            user => user.UserId == userId && user.Role == "Customer",
            cancellationToken);
    }

    public async Task AddAsync(UserInformation customer, CancellationToken cancellationToken)
    {
        await _context.UserInformation.AddAsync(customer, cancellationToken);
    }

    public Task<UserInformation?> GetCustomerForUpdateAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        return _context.UserInformation.SingleOrDefaultAsync(
            user => user.UserId == userId && user.Role == "Customer",
            cancellationToken);
    }

    public Task<AdminCustomerAddressDto?> GetAddressAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        return _context.UserInformation
            .AsNoTracking()
            .Where(user => user.UserId == userId && user.Role == "Customer")
            .Select(AdminOrderCustomerMappings.CustomerAddress)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResultDto<AdminCustomerDto>> SearchAsync(
        AdminCustomerSearchRequestDto request,
        CancellationToken cancellationToken)
    {
        var customers = _context.UserInformation
            .AsNoTracking()
            .Where(user => user.Role == "Customer");

        string? keyword = string.IsNullOrWhiteSpace(request.Keyword) ? null : request.Keyword.Trim();
        if (keyword is not null)
        {
            customers = customers.Where(user =>
                user.UserName.Contains(keyword)
                || user.Email.Contains(keyword)
                || (user.PhoneNumber != null && user.PhoneNumber.Contains(keyword)));
        }

        if (request.IsActive.HasValue)
        {
            customers = customers.Where(user => user.IsActive == request.IsActive.Value);
        }

        int totalItems = await customers.CountAsync(cancellationToken);
        var items = await customers
            .OrderBy(user => user.UserId)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(AdminOrderCustomerMappings.Customer)
            .ToArrayAsync(cancellationToken);

        return new PagedResultDto<AdminCustomerDto>
        {
            Items = items,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<PagedResultDto<AdminCustomerOrderDto>> GetOrdersAsync(
        int userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var orders = _context.Orders
            .AsNoTracking()
            .Where(order => order.UserId == userId);

        int totalItems = await orders.CountAsync(cancellationToken);
        var items = await orders
            .OrderByDescending(order => order.OrderDate)
            .ThenByDescending(order => order.OrderId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(AdminOrderCustomerMappings.CustomerOrder)
            .ToArrayAsync(cancellationToken);

        return new PagedResultDto<AdminCustomerOrderDto>
        {
            Items = items,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
