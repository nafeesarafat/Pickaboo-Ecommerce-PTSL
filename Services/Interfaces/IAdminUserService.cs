using Ecommerce.Api.DTOs.Admin;

namespace Ecommerce.Api.Services;

public interface IAdminUserService
{
    Task<IReadOnlyList<AdminUserDto>> GetAllUsersAsync(CancellationToken cancellationToken);
    Task<AdminUserDetailsDto> GetUserDetailsAsync(int userId, CancellationToken cancellationToken);
    Task<AdminUserDto> UpdateUserAsync(
        int userId,
        UpdateAdminUserRequestDto request,
        CancellationToken cancellationToken);
}
