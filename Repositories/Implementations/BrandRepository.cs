using Ecommerce.Api.Data;
using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Repositories;

public class BrandRepository : IBrandRepository
{
    private readonly AppDbContext _context;

    public BrandRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Brand?> GetByIdAsync(int brandId, CancellationToken cancellationToken)
    {
        return _context.Brands
            .SingleOrDefaultAsync(brand => brand.BrandId == brandId, cancellationToken);
    }

    public Task<bool> NameExistsAsync(
        string brandName,
        int? excludedBrandId,
        CancellationToken cancellationToken)
    {
        return _context.Brands.AnyAsync(
            brand => brand.BrandName == brandName
                && (!excludedBrandId.HasValue || brand.BrandId != excludedBrandId.Value),
            cancellationToken);
    }

    public async Task AddAsync(Brand brand, CancellationToken cancellationToken)
    {
        await _context.Brands.AddAsync(brand, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
