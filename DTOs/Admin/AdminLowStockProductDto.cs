namespace Ecommerce.Api.DTOs.Admin;

public class AdminLowStockProductDto : AdminProductStockDto
{
    public int Threshold { get; set; }
}
