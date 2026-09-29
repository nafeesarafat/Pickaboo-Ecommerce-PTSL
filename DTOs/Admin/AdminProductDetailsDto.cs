namespace Ecommerce.Api.DTOs.Admin;

public class AdminProductDetailsDto : AdminProductResponseDto
{
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}