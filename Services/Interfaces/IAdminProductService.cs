using Ecommerce.Api.DTOs.Admin;

namespace Ecommerce.Api.Services;

public interface IAdminProductService
{
    Task<IReadOnlyList<AdminProductResponseDto>> GetAllProductsAsync(CancellationToken cancellationToken);
    Task<AdminProductDetailsDto> GetProductDetailsAsync(int productId, CancellationToken cancellationToken);
    Task<AdminProductResponseDto> AddProductAsync(AddProductRequestDto request, CancellationToken cancellationToken);
    Task<AdminProductResponseDto> UpdateProductAsync(int productId, UpdateProductRequestDto request, CancellationToken cancellationToken);
    Task DeleteProductAsync(int productId, CancellationToken cancellationToken);
}