using Konta.Identity.Models;

namespace Konta.Identity.Data.Repositories.Interfaces;

public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(Guid permissionId);
    Task<IEnumerable<string>> GetPermissionsByUserIdAsync(Guid userId);
    Task<Permission?> GetBySystemNameAsync(string systemName);
    Task<IEnumerable<string>> GetAllPermissionSystemNamesAsync();
}
