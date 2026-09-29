using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Api.Models;

public class AuditLog
{
    public int AuditLogId { get; set; }

    public int AdministratorId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime Timestamp { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;

    [ForeignKey(nameof(AdministratorId))]
    public Administrator Administrator { get; set; } = null!;
}
