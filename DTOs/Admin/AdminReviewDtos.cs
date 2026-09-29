using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Admin;

public class AdminReviewSearchRequestDto
{
    public int? ReviewId { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedDate { get; set; }
}

public class AdminReviewDto
{
    public int ReviewId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class AdminReviewDetailsDto
{
    public int ReviewId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class RejectReviewRequestDto
{
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}

public class ReviewStatusDto
{
    public int ReviewId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
}
