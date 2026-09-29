using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs.Admin;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Services;

public class BrandService : IBrandService
{
    private readonly AppDbContext _context;
    public BrandService(AppDbContext context) => _context = context;
    public async Task<IReadOnlyList<BrandDto>> GetAllAsync(int? brandId, string? brandName, string? description, string? imageUrl, bool? isActive, CancellationToken cancellationToken)
    {
        var query = _context.Brands.AsNoTracking().AsQueryable();
        if (brandId.HasValue) query = query.Where(x => x.BrandId == brandId);
        if (!string.IsNullOrWhiteSpace(brandName)) query = query.Where(x => x.BrandName.Contains(brandName));
        if (!string.IsNullOrWhiteSpace(description)) query = query.Where(x => x.Description != null && x.Description.Contains(description));
        if (!string.IsNullOrWhiteSpace(imageUrl)) query = query.Where(x => x.ImageUrl != null && x.ImageUrl.Contains(imageUrl));
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive);
        return await query.OrderBy(x => x.BrandName).Select(x => new BrandDto { BrandId = x.BrandId, BrandName = x.BrandName, Description = x.Description, ImageUrl = x.ImageUrl, IsActive = x.IsActive }).ToArrayAsync(cancellationToken);
    }
}
