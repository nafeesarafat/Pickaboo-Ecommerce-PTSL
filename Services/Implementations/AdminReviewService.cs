using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.DTOs.Product;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Services;

public class AdminReviewService : IAdminReviewService
{
    private readonly AppDbContext _context;

    public AdminReviewService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AdminReviewDto>> GetReviewsAsync(
        AdminReviewSearchRequestDto request,
        CancellationToken cancellationToken)
    {
        IQueryable<Review> reviews = _context.Reviews.AsNoTracking();
        if (request.ReviewId.HasValue)
        {
            reviews = reviews.Where(review => review.ReviewId == request.ReviewId.Value);
        }

        if (request.ProductId.HasValue)
        {
            reviews = reviews.Where(review => review.ProductId == request.ProductId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.ProductName))
        {
            string productName = request.ProductName.Trim();
            reviews = reviews.Where(review => review.Product.ProductName.Contains(productName));
        }

        if (request.UserId.HasValue)
        {
            reviews = reviews.Where(review => review.UserId == request.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.UserName))
        {
            string userName = request.UserName.Trim();
            reviews = reviews.Where(review => review.User.UserName.Contains(userName));
        }

        if (request.Rating.HasValue)
        {
            reviews = reviews.Where(review => review.Rating == request.Rating.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Comment))
        {
            string comment = request.Comment.Trim();
            reviews = reviews.Where(review => review.Comment != null && review.Comment.Contains(comment));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            string status = request.Status.Trim();
            reviews = reviews.Where(review => review.Status == status);
        }

        if (request.CreatedDate.HasValue)
        {
            reviews = reviews.Where(review => review.CreatedDate == request.CreatedDate.Value);
        }

        return await reviews
            .OrderByDescending(review => review.CreatedDate)
            .Select(review => new AdminReviewDto
            {
                ReviewId = review.ReviewId,
                ProductId = review.ProductId,
                ProductName = review.Product.ProductName,
                UserId = review.UserId,
                UserName = review.User.UserName,
                Rating = review.Rating,
                Comment = review.Comment,
                Status = review.Status,
                RejectionReason = review.RejectionReason,
                CreatedDate = review.CreatedDate
            })
            .ToArrayAsync(cancellationToken);
    }

    public async Task<AdminReviewDetailsDto> GetReviewAsync(
        int reviewId,
        CancellationToken cancellationToken)
    {
        ValidateId(reviewId, "review");
        return await _context.Reviews
            .AsNoTracking()
            .Where(review => review.ReviewId == reviewId)
            .Select(review => new AdminReviewDetailsDto
            {
                ReviewId = review.ReviewId,
                ProductId = review.ProductId,
                ProductName = review.Product.ProductName,
                UserName = review.User.UserName,
                Rating = review.Rating,
                Comment = review.Comment,
                Status = review.Status,
                RejectionReason = review.RejectionReason,
                CreatedDate = review.CreatedDate
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Review with id {reviewId} was not found.");
    }

    public async Task<ReviewStatusDto> ApproveReviewAsync(
        int reviewId,
        CancellationToken cancellationToken)
    {
        var review = await GetExistingReviewAsync(reviewId, cancellationToken);
        review.Status = "Approved";
        review.RejectionReason = null;
        await _context.SaveChangesAsync(cancellationToken);
        return ToStatusDto(review);
    }

    public async Task<ReviewStatusDto> RejectReviewAsync(
        int reviewId,
        RejectReviewRequestDto request,
        CancellationToken cancellationToken)
    {
        var reason = request.Reason.Trim();
        if (reason.Length == 0)
        {
            throw new BadRequestException("A rejection reason is required.");
        }

        var review = await GetExistingReviewAsync(reviewId, cancellationToken);
        review.Status = "Rejected";
        review.RejectionReason = reason;
        await _context.SaveChangesAsync(cancellationToken);
        return ToStatusDto(review);
    }

    public async Task DeleteReviewAsync(int reviewId, CancellationToken cancellationToken)
    {
        var review = await GetExistingReviewAsync(reviewId, cancellationToken);
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProductRatingsDto> GetProductRatingsAsync(
        int productId,
        CancellationToken cancellationToken)
    {
        ValidateId(productId, "product");
        if (!await _context.Products.AnyAsync(product => product.ProductId == productId, cancellationToken))
        {
            throw new NotFoundException($"Product with id {productId} was not found.");
        }

        var approvedReviews = _context.Reviews
            .AsNoTracking()
            .Where(review => review.ProductId == productId && review.Status == "Approved");
        int totalReviews = await approvedReviews.CountAsync(cancellationToken);
        decimal averageRating = totalReviews == 0
            ? 0m
            : await approvedReviews.AverageAsync(review => (decimal)review.Rating, cancellationToken);

        return new ProductRatingsDto
        {
            ProductId = productId,
            AverageRating = averageRating,
            TotalReviews = totalReviews
        };
    }

    private async Task<Review> GetExistingReviewAsync(int reviewId, CancellationToken cancellationToken)
    {
        ValidateId(reviewId, "review");
        return await _context.Reviews
            .SingleOrDefaultAsync(review => review.ReviewId == reviewId, cancellationToken)
            ?? throw new NotFoundException($"Review with id {reviewId} was not found.");
    }

    private static void ValidateId(int id, string resource)
    {
        if (id <= 0)
        {
            throw new BadRequestException($"A valid {resource} id is required.");
        }
    }

    private static ReviewStatusDto ToStatusDto(Review review)
    {
        return new ReviewStatusDto
        {
            ReviewId = review.ReviewId,
            Status = review.Status,
            Reason = review.RejectionReason
        };
    }
}
