using Ecommerce.Api.DTOs.Admin;

namespace Ecommerce.Api.Services;

public interface IAdministratorService
{
    Task<PagedResultDto<AdministratorDto>> GetAllAsync(AdministratorQueryDto query, CancellationToken cancellationToken);
    Task<AdministratorDto> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<AdministratorDto> CreateAsync(CreateAdministratorRequestDto request, CancellationToken cancellationToken);
}
