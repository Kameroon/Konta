using Konta.Identity.Models;

namespace Konta.Identity.Data.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<Guid> CreateAsync(Role role);
    Task<Role?> GetByIdAsync(Guid roleId);
    Task<Role?> GetByNameAsync(Guid tenantId, string roleName);
    Task<IEnumerable<Role>> GetByUserIdAsync(Guid userId);
    Task AddPermissionToRoleAsync(Guid roleId, Guid permissionId);
    Task AssignRoleToUserAsync(Guid userId, Guid roleId);
}
