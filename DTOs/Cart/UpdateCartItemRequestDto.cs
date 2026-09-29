using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Cart;

public class UpdateCartItemRequestDto
{
    [Required(ErrorMessage = "StockQuantity is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "StockQuantity must be a positive integer.")]
    public int StockQuantity { get; set; }

    /// <summary>
    /// Server-controlled timestamp. Any value supplied by the client is ignored.
    /// </summary>
    public DateTime UpdatedDate { get; set; }
}
