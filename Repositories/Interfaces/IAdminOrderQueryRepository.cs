using Ecommerce.Api.DTOs.Admin;

namespace Ecommerce.Api.Repositories;

public interface IAdminOrderQueryRepository
{
    Task<IReadOnlyList<AdminOrderViewDto>> GetByStatusAsync(
        string status,
        CancellationToken cancellationToken);

    Task<PagedResultDto<AdminOrderViewDto>> SearchAsync(
        AdminOrderSearchRequestDto request,
        CancellationToken cancellationToken);
}
