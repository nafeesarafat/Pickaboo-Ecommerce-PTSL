using Ecommerce.Api.Models;

namespace Ecommerce.Api.Repositories;

public interface IBrandRepository
{
    Task<Brand?> GetByIdAsync(int brandId, CancellationToken cancellationToken);
    Task<bool> NameExistsAsync(string brandName, int? excludedBrandId, CancellationToken cancellationToken);
    Task AddAsync(Brand brand, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
