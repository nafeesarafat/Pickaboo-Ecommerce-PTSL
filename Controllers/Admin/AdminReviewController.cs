using System.ComponentModel.DataAnnotations;
using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/reviews")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class AdminReviewController : ControllerBase
{
    private readonly IAdminReviewService _reviewService;

    public AdminReviewController(IAdminReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AdminReviewDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] AdminReviewSearchRequestDto request,
        CancellationToken cancellationToken)
    {
        var reviews = await _reviewService.GetReviewsAsync(request, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminReviewDto>>.Ok(
            reviews, "Reviews retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AdminReviewDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute, Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        var review = await _reviewService.GetReviewAsync(id, cancellationToken);
        return Ok(ApiResponse<AdminReviewDetailsDto>.Ok(review, "Review retrieved successfully."));
    }

    [HttpPut("{id:int}/approve")]
    [ProducesResponseType(typeof(ApiResponse<ReviewStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(
        [FromRoute, Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        var review = await _reviewService.ApproveReviewAsync(id, cancellationToken);
        return Ok(ApiResponse<ReviewStatusDto>.Ok(review, "Review approved successfully."));
    }

    [HttpPut("{id:int}/reject")]
    [ProducesResponseType(typeof(ApiResponse<ReviewStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(
        [FromRoute, Range(1, int.MaxValue)] int id,
        [FromBody] RejectReviewRequestDto request,
        CancellationToken cancellationToken)
    {
        var review = await _reviewService.RejectReviewAsync(id, request, cancellationToken);
        return Ok(ApiResponse<ReviewStatusDto>.Ok(review, "Review rejected successfully."));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute, Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        await _reviewService.DeleteReviewAsync(id, cancellationToken);
        return NoContent();
    }
}
