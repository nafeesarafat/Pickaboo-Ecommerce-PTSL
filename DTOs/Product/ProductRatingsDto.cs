namespace Ecommerce.Api.DTOs.Product;

public class ProductRatingsDto
{
    public int ProductId { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
}
