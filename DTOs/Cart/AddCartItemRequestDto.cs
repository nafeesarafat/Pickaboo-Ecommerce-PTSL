using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Cart;

public class AddCartItemRequestDto
{
    /// <summary>
    /// Client-supplied value is ignored. The authenticated user is always taken from the JWT.
    /// </summary>
    public int UserId { get; set; }

    [Required(ErrorMessage = "ProductId is required.")]
    public long ProductId { get; set; }

    [Required(ErrorMessage = "StockQuantity is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "StockQuantity must be a positive integer.")]
    public int StockQuantity { get; set; }
}
