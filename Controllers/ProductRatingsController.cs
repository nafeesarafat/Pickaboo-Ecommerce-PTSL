using System.ComponentModel.DataAnnotations;
using Ecommerce.Api.DTOs.Product;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
public class ProductRatingsController : ControllerBase
{
    private readonly IAdminReviewService _reviewService;

    public ProductRatingsController(IAdminReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("{productId:int}/ratings")]
    [ProducesResponseType(typeof(ApiResponse<ProductRatingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRatings(
        [FromRoute, Range(1, int.MaxValue)] int productId,
        CancellationToken cancellationToken)
    {
        var ratings = await _reviewService.GetProductRatingsAsync(productId, cancellationToken);
        return Ok(ApiResponse<ProductRatingsDto>.Ok(ratings, "Product ratings retrieved successfully."));
    }
}
