using Ecommerce.Api.DTOs.Admin;

namespace Ecommerce.Api.Services;

public interface IBrandService
{
    Task<IReadOnlyList<BrandDto>> GetAllAsync(int? brandId, string? brandName, string? description, string? imageUrl, bool? isActive, CancellationToken cancellationToken);
}
