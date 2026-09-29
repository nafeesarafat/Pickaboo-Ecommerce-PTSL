using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Models;

namespace Ecommerce.Api.Repositories;

public interface IAdminCustomerRepository
{
    Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
    Task<bool> CustomerExistsAsync(int userId, CancellationToken cancellationToken);
    Task AddAsync(UserInformation customer, CancellationToken cancellationToken);
    Task<UserInformation?> GetCustomerForUpdateAsync(int userId, CancellationToken cancellationToken);
    Task<AdminCustomerAddressDto?> GetAddressAsync(int userId, CancellationToken cancellationToken);
    Task<PagedResultDto<AdminCustomerDto>> SearchAsync(
        AdminCustomerSearchRequestDto request,
        CancellationToken cancellationToken);
    Task<PagedResultDto<AdminCustomerOrderDto>> GetOrdersAsync(
        int userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
