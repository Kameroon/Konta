using Konta.Identity.Models;

namespace Konta.Identity.Data.Repositories.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id);
    Task<Tenant?> GetByNameAsync(string name);
    Task<IEnumerable<Tenant>> GetAllAsync();
    Task<Guid> CreateAsync(Tenant tenant);
    Task<bool> UpdateAsync(Tenant tenant);
    Task<bool> DeleteAsync(Guid id);
}
