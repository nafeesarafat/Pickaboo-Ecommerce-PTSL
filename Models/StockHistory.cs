using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Api.Models;

public class StockHistory
{
    public int StockHistoryId { get; set; }

    public int ProductId { get; set; }

    /// <summary>Signed quantity change. Negative when stock is reduced.</summary>
    public int Change { get; set; }

    [MaxLength(255)]
    public string? Reason { get; set; }

    /// <summary>Identifier of whoever caused the change (user or administrator id).</summary>
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    public DateTime Timestamp { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = null!;
}
