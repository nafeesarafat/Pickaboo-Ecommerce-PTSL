using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Services;

public class AdministratorService : IAdministratorService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditLogService _auditLogService;

    public AdministratorService(AppDbContext context, IPasswordHasher passwordHasher, IAuditLogService auditLogService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResultDto<AdministratorDto>> GetAllAsync(AdministratorQueryDto query, CancellationToken cancellationToken)
    {
        var items = _context.Administrators.AsNoTracking().AsQueryable();
        if (query.AdministratorId.HasValue) items = items.Where(x => x.AdministratorId == query.AdministratorId);
        if (!string.IsNullOrWhiteSpace(query.Keyword)) items = items.Where(x => x.FullName.Contains(query.Keyword) || x.Email.Contains(query.Keyword) || x.Username.Contains(query.Keyword));
        if (!string.IsNullOrWhiteSpace(query.FullName)) items = items.Where(x => x.FullName.Contains(query.FullName));
        if (!string.IsNullOrWhiteSpace(query.Email)) items = items.Where(x => x.Email.Contains(query.Email));
        if (!string.IsNullOrWhiteSpace(query.PhoneNumber)) items = items.Where(x => x.PhoneNumber != null && x.PhoneNumber.Contains(query.PhoneNumber));
        if (!string.IsNullOrWhiteSpace(query.Username)) items = items.Where(x => x.Username.Contains(query.Username));
        if (!string.IsNullOrWhiteSpace(query.Status)) items = items.Where(x => x.AccountStatus == query.Status);
        if (!string.IsNullOrWhiteSpace(query.AccountStatus)) items = items.Where(x => x.AccountStatus == query.AccountStatus);
        if (query.RoleId.HasValue) items = items.Where(x => x.RoleId == query.RoleId);
        if (!string.IsNullOrWhiteSpace(query.RoleName))
        {
            items = string.Equals(query.RoleName.Trim(), "Admin", StringComparison.OrdinalIgnoreCase)
                ? items.Where(x => x.RoleId == 1)
                : int.TryParse(query.RoleName.Replace("Role ", "", StringComparison.OrdinalIgnoreCase), out var roleId)
                    ? items.Where(x => x.RoleId == roleId)
                    : items.Where(x => false);
        }
        if (query.LastLogin.HasValue) items = items.Where(x => x.LastLogin == query.LastLogin);
        var total = await items.CountAsync(cancellationToken);
        var pageSize = query.PageSize;
        var pageNumber = query.PageNumber;
        var data = await items.OrderBy(x => x.AdministratorId).Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(ToDtoExpression()).ToArrayAsync(cancellationToken);
        return new PagedResultDto<AdministratorDto> { Items = data, TotalItems = total, TotalPages = (int)Math.Ceiling(total / (double)pageSize), PageNumber = pageNumber, PageSize = pageSize };
    }

    public async Task<AdministratorDto> GetByIdAsync(int id, CancellationToken cancellationToken)
        => await _context.Administrators.AsNoTracking().Where(x => x.AdministratorId == id).Select(ToDtoExpression()).SingleOrDefaultAsync(cancellationToken)
           ?? throw new NotFoundException($"Administrator with id {id} was not found.");

    public async Task<AdministratorDto> CreateAsync(CreateAdministratorRequestDto request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim();
        if (await _context.Administrators.AnyAsync(x => x.Username == username || x.Email == email, cancellationToken))
            throw new ConflictException("An administrator with this username or email already exists.");
        var administrator = new Administrator
        {
            FullName = request.FullName.Trim(), Email = email, PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            Username = username, PasswordHash = _passwordHasher.HashPassword(request.Password), RoleId = request.RoleId,
            AccountStatus = request.AccountStatus.Trim(), CreatedDate = DateTime.UtcNow
        };
        _context.Administrators.Add(administrator);
        await _context.SaveChangesAsync(cancellationToken);
        await _auditLogService.RecordAsync(administrator.AdministratorId, "Create Administrator", $"Administrator {administrator.AdministratorId} was created.", "Success", cancellationToken);
        return ToDto(administrator);
    }

    private static AdministratorDto ToDto(Administrator x) => new()
    {
        AdministratorId = x.AdministratorId, FullName = x.FullName, Email = x.Email, PhoneNumber = x.PhoneNumber,
        Username = x.Username, RoleId = x.RoleId, RoleName = x.RoleId == 1 ? "Admin" : $"Role {x.RoleId}",
        AccountStatus = x.AccountStatus, LastLogin = x.LastLogin, CreatedDate = x.CreatedDate
    };
    private static System.Linq.Expressions.Expression<Func<Administrator, AdministratorDto>> ToDtoExpression() =>
        x => new AdministratorDto { AdministratorId = x.AdministratorId, FullName = x.FullName, Email = x.Email, PhoneNumber = x.PhoneNumber, Username = x.Username, RoleId = x.RoleId, RoleName = x.RoleId == 1 ? "Admin" : "Role " + x.RoleId, AccountStatus = x.AccountStatus, LastLogin = x.LastLogin, CreatedDate = x.CreatedDate };
}
