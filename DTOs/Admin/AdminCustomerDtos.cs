using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Admin;

public class AddAdminCustomerRequestDto
{
    [Required, MaxLength(100)]
    public string UserName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [Required, MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}

public class AdminCustomerSearchRequestDto
{
    public string? Keyword { get; set; }
    public bool? IsActive { get; set; }

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
}

public class UpdateAdminCustomerStatusRequestDto
{
    public bool IsActive { get; set; }
}

public class AdminCustomerDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool IsActive { get; set; }
    public DateTime RegistrationDate { get; set; }
}

public class AdminCustomerOrderDto
{
    public int UserId { get; set; }
    public int OrderId { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
}

public class AdminCustomerAddressDto
{
    public int UserId { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
}

public class AdminCustomerStatusDto
{
    public int UserId { get; set; }
    public bool IsActive { get; set; }
}
