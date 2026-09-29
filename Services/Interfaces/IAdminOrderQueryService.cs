using Ecommerce.Api.DTOs.Admin;

namespace Ecommerce.Api.Services;

public interface IAdminOrderQueryService
{
    Task<IReadOnlyList<AdminOrderViewDto>> GetByStatusAsync(
        string status,
        CancellationToken cancellationToken);
    Task<PagedResultDto<AdminOrderViewDto>> SearchAsync(
        AdminOrderSearchRequestDto request,
        CancellationToken cancellationToken);
}
