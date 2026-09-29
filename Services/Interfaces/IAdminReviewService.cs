using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.DTOs.Product;

namespace Ecommerce.Api.Services;

public interface IAdminReviewService
{
    Task<IReadOnlyList<AdminReviewDto>> GetReviewsAsync(
        AdminReviewSearchRequestDto request,
        CancellationToken cancellationToken);
    Task<AdminReviewDetailsDto> GetReviewAsync(int reviewId, CancellationToken cancellationToken);
    Task<ReviewStatusDto> ApproveReviewAsync(int reviewId, CancellationToken cancellationToken);
    Task<ReviewStatusDto> RejectReviewAsync(
        int reviewId,
        RejectReviewRequestDto request,
        CancellationToken cancellationToken);
    Task DeleteReviewAsync(int reviewId, CancellationToken cancellationToken);
    Task<ProductRatingsDto> GetProductRatingsAsync(int productId, CancellationToken cancellationToken);
}
