using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Models;
using Ecommerce.Api.Repositories;

namespace Ecommerce.Api.Services;

public class AdminBrandService : IAdminBrandService
{
    private readonly IBrandRepository _brandRepository;

    public AdminBrandService(IBrandRepository brandRepository)
    {
        _brandRepository = brandRepository;
    }

    public async Task<AdminBrandDto> GetBrandAsync(int brandId, CancellationToken cancellationToken)
    {
        ValidateBrandId(brandId);
        var brand = await GetExistingBrandAsync(brandId, cancellationToken);
        return ToDto(brand);
    }

    public async Task<AdminBrandDto> AddBrandAsync(
        AddBrandRequestDto request,
        CancellationToken cancellationToken)
    {
        string brandName = NormalizeBrandName(request.BrandName);
        await EnsureBrandNameIsAvailableAsync(brandName, null, cancellationToken);

        var brand = new Brand
        {
            BrandName = brandName,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            IsActive = request.IsActive,
            UpdatedDate = DateTime.UtcNow
        };

        await _brandRepository.AddAsync(brand, cancellationToken);
        await _brandRepository.SaveChangesAsync(cancellationToken);
        return ToDto(brand);
    }

    public async Task<AdminBrandDto> UpdateBrandAsync(
        int brandId,
        UpdateBrandRequestDto request,
        CancellationToken cancellationToken)
    {
        ValidateBrandId(brandId);
        var brand = await GetExistingBrandAsync(brandId, cancellationToken);
        string brandName = NormalizeBrandName(request.BrandName);
        if (request.UpdatedDate == default)
        {
            throw new BadRequestException("A valid updated date is required.");
        }

        await EnsureBrandNameIsAvailableAsync(brandName, brandId, cancellationToken);

        brand.BrandName = brandName;
        brand.Description = request.Description;
        brand.ImageUrl = request.ImageUrl;
        brand.IsActive = request.IsActive;
        brand.UpdatedDate = request.UpdatedDate;

        await _brandRepository.SaveChangesAsync(cancellationToken);
        return ToDto(brand);
    }

    public async Task DeleteBrandAsync(int brandId, CancellationToken cancellationToken)
    {
        ValidateBrandId(brandId);
        var brand = await GetExistingBrandAsync(brandId, cancellationToken);
        brand.IsActive = false;
        await _brandRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<AdminBrandDto> UpdateBrandStatusAsync(
        int brandId,
        UpdateBrandStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        ValidateBrandId(brandId);
        var brand = await GetExistingBrandAsync(brandId, cancellationToken);
        brand.IsActive = request.IsActive;
        await _brandRepository.SaveChangesAsync(cancellationToken);
        return ToDto(brand);
    }

    private async Task<Brand> GetExistingBrandAsync(int brandId, CancellationToken cancellationToken)
    {
        return await _brandRepository.GetByIdAsync(brandId, cancellationToken)
            ?? throw new NotFoundException($"Brand with id {brandId} was not found.");
    }

    private async Task EnsureBrandNameIsAvailableAsync(
        string brandName,
        int? excludedBrandId,
        CancellationToken cancellationToken)
    {
        if (await _brandRepository.NameExistsAsync(brandName, excludedBrandId, cancellationToken))
        {
            throw new ConflictException("A brand with this name already exists.");
        }
    }

    private static void ValidateBrandId(int brandId)
    {
        if (brandId <= 0)
        {
            throw new BadRequestException("A valid brand id is required.");
        }
    }

    private static string NormalizeBrandName(string brandName)
    {
        string normalized = brandName.Trim();
        if (normalized.Length == 0)
        {
            throw new BadRequestException("A brand name is required.");
        }

        return normalized;
    }

    private static AdminBrandDto ToDto(Brand brand)
    {
        return new AdminBrandDto
        {
            BrandId = brand.BrandId,
            BrandName = brand.BrandName,
            Description = brand.Description,
            ImageUrl = brand.ImageUrl,
            IsActive = brand.IsActive
        };
    }
}
