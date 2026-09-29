using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.DTOs.Category;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CategoryListDto>> GetCategoriesAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .Select(category => new CategoryListDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                ProductCount = category.Products.Count(product => product.IsActive),
                IsActive = category.IsActive
            })
            .OrderBy(category => category.CategoryName)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<CategoryDetailsDto> GetCategoryByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        ValidateCategoryId(id);

        var category = await _context.Categories
            .AsNoTracking()
            .Where(category => category.CategoryId == id && category.IsActive)
            .Select(category => new CategoryDetailsDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                IsActive = category.IsActive,
                ProductCount = category.Products.Count(product => product.IsActive)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return category ?? throw new NotFoundException($"Category with id {id} was not found.");
    }

    public async Task<IReadOnlyList<CategoryProductDto>> GetCategoryProductsAsync(
        int id,
        CancellationToken cancellationToken)
    {
        ValidateCategoryId(id);

        var category = await _context.Categories
            .AsNoTracking()
            .Where(category => category.CategoryId == id && category.IsActive)
            .Select(category => new { category.CategoryId, category.CategoryName })
            .FirstOrDefaultAsync(cancellationToken);

        if (category is null)
        {
            throw new NotFoundException($"Category with id {id} was not found.");
        }

        return await CategoryProductQuery.GetActiveProducts(_context, id)
            .Select(product => new CategoryProductDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                BrandId = product.BrandId,
                BrandName = product.Brand.BrandName
            })
            .OrderBy(product => product.ProductName)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminCategoryDto>> GetAllCategoriesForAdminAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Categories
            .AsNoTracking()
            .Select(category => new AdminCategoryDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                ProductCount = category.Products.Count(),
                IsActive = category.IsActive
            })
            .OrderBy(category => category.CategoryName)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<AdminCategoryDto> GetCategoryForAdminAsync(
        int id,
        CancellationToken cancellationToken)
    {
        ValidateCategoryId(id);

        var category = await _context.Categories
            .AsNoTracking()
            .Where(item => item.CategoryId == id)
            .Select(item => new AdminCategoryDto
            {
                CategoryId = item.CategoryId,
                CategoryName = item.CategoryName,
                Description = item.Description,
                ImageUrl = item.ImageUrl,
                ProductCount = item.Products.Count(),
                IsActive = item.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        return category ?? throw new NotFoundException($"Category with id {id} was not found.");
    }

    public async Task<AdminCategoryDto> AddCategoryAsync(
        AddCategoryRequestDto request,
        CancellationToken cancellationToken)
    {
        string categoryName = NormalizeCategoryName(request.CategoryName);
        await EnsureCategoryNameIsAvailableAsync(categoryName, null, cancellationToken);

        var category = new Category
        {
            CategoryName = categoryName,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            IsActive = request.IsActive
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return ToAdminDto(category, 0);
    }

    public async Task<AdminCategoryDto> UpdateCategoryAsync(
        int id,
        UpdateCategoryRequestDto request,
        CancellationToken cancellationToken)
    {
        ValidateCategoryId(id);

        var category = await _context.Categories
            .SingleOrDefaultAsync(item => item.CategoryId == id, cancellationToken);
        if (category is null)
        {
            throw new NotFoundException($"Category with id {id} was not found.");
        }

        string categoryName = NormalizeCategoryName(request.CategoryName);
        await EnsureCategoryNameIsAvailableAsync(categoryName, id, cancellationToken);

        category.CategoryName = categoryName;
        category.Description = request.Description;
        category.ImageUrl = request.ImageUrl;
        category.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        int productCount = await _context.Products
            .CountAsync(product => product.CategoryId == id, cancellationToken);
        return ToAdminDto(category, productCount);
    }

    public async Task DeleteCategoryAsync(int id, CancellationToken cancellationToken)
    {
        ValidateCategoryId(id);

        var category = await _context.Categories
            .SingleOrDefaultAsync(item => item.CategoryId == id, cancellationToken);
        if (category is null)
        {
            throw new NotFoundException($"Category with id {id} was not found.");
        }

        category.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateCategoryId(int id)
    {
        if (id <= 0)
        {
            throw new BadRequestException("A valid category id is required.");
        }
    }

    private async Task EnsureCategoryNameIsAvailableAsync(
        string categoryName,
        int? excludedCategoryId,
        CancellationToken cancellationToken)
    {
        bool nameInUse = await _context.Categories
            .AnyAsync(category =>
                category.CategoryName == categoryName
                && (!excludedCategoryId.HasValue || category.CategoryId != excludedCategoryId.Value),
                cancellationToken);

        if (nameInUse)
        {
            throw new ConflictException("A category with this name already exists.");
        }
    }

    private static string NormalizeCategoryName(string categoryName)
    {
        string normalized = categoryName.Trim();
        if (normalized.Length == 0)
        {
            throw new BadRequestException("A category name is required.");
        }

        return normalized;
    }

    private static AdminCategoryDto ToAdminDto(Category category, int productCount)
    {
        return new AdminCategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            ProductCount = productCount,
            IsActive = category.IsActive
        };
    }
}
