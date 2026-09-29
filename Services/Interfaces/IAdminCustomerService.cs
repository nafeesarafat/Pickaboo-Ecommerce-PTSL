using Ecommerce.Api.DTOs.Admin;

namespace Ecommerce.Api.Services;

public interface IAdminCustomerService
{
    Task<AdminCustomerDto> AddAsync(
        AddAdminCustomerRequestDto request,
        CancellationToken cancellationToken);
    Task<AdminCustomerStatusDto> DeleteAsync(int userId, CancellationToken cancellationToken);
    Task<PagedResultDto<AdminCustomerOrderDto>> GetOrdersAsync(
        int userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
    Task<AdminCustomerAddressDto> GetAddressAsync(int userId, CancellationToken cancellationToken);
    Task<AdminCustomerStatusDto> UpdateStatusAsync(
        int userId,
        bool isActive,
        CancellationToken cancellationToken);
    Task<PagedResultDto<AdminCustomerDto>> SearchAsync(
        AdminCustomerSearchRequestDto request,
        CancellationToken cancellationToken);
}
