using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.Models;

public class SystemLog
{
    public int LogId { get; set; }

    [Required]
    [MaxLength(20)]
    public string Level { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string Message { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }
}
