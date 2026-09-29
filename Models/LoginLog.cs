using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Api.Models;

public class LoginLog
{
    public int LogId { get; set; }

    public int AdministratorId { get; set; }

    [MaxLength(200)]
    public string? AdministratorName { get; set; }

    public DateTime LoginDateTime { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;

    [ForeignKey(nameof(AdministratorId))]
    public Administrator Administrator { get; set; } = null!;
}
