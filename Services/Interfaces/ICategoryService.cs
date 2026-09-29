using Ecommerce.Api.DTOs.Category;
using Ecommerce.Api.DTOs.Admin;

namespace Ecommerce.Api.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryListDto>> GetCategoriesAsync(CancellationToken cancellationToken);

    Task<CategoryDetailsDto> GetCategoryByIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<CategoryProductDto>> GetCategoryProductsAsync(
        int id,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminCategoryDto>> GetAllCategoriesForAdminAsync(
        CancellationToken cancellationToken);

    Task<AdminCategoryDto> GetCategoryForAdminAsync(int id, CancellationToken cancellationToken);

    Task<AdminCategoryDto> AddCategoryAsync(
        AddCategoryRequestDto request,
        CancellationToken cancellationToken);

    Task<AdminCategoryDto> UpdateCategoryAsync(
        int id,
        UpdateCategoryRequestDto request,
        CancellationToken cancellationToken);

    Task DeleteCategoryAsync(int id, CancellationToken cancellationToken);
}
