using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.Models;

public class Administrator
{
    public int AdministratorId { get; set; }

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    public int RoleId { get; set; }

    [Required]
    [MaxLength(20)]
    public string AccountStatus { get; set; } = "Active";

    public DateTime? LastLogin { get; set; }

    public DateTime CreatedDate { get; set; }

    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public ICollection<LoginLog> LoginLogs { get; set; } = new List<LoginLog>();
}
