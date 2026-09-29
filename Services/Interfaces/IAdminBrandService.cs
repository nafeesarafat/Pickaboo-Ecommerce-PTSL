using Ecommerce.Api.DTOs.Admin;

namespace Ecommerce.Api.Services;

public interface IAdminBrandService
{
    Task<AdminBrandDto> GetBrandAsync(int brandId, CancellationToken cancellationToken);
    Task<AdminBrandDto> AddBrandAsync(AddBrandRequestDto request, CancellationToken cancellationToken);
    Task<AdminBrandDto> UpdateBrandAsync(
        int brandId,
        UpdateBrandRequestDto request,
        CancellationToken cancellationToken);
    Task DeleteBrandAsync(int brandId, CancellationToken cancellationToken);
    Task<AdminBrandDto> UpdateBrandStatusAsync(
        int brandId,
        UpdateBrandStatusRequestDto request,
        CancellationToken cancellationToken);
}
