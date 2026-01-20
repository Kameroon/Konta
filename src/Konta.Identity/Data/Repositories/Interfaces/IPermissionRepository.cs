using Konta.Identity.Models;

namespace Konta.Identity.Data.Repositories.Interfaces;

public interface IPermissionRepository
{
    Task<IEnumerable<string>> GetPermissionsByUserIdAsync(Guid userId);
    Task<Permission?> GetBySystemNameAsync(string systemName);
}
