using Konta.Finance.Core.Models;

namespace Konta.Finance.Core.Data.Repositories.Interfaces;

public interface ITierRepository
{
    Task<Tier?> GetByIdAsync(Guid id);
    Task<IEnumerable<Tier>> GetByTenantIdAsync(Guid tenantId, TierType? type = null);
    Task<Guid> CreateAsync(Tier tier);
    Task<bool> UpdateAsync(Tier tier);
    Task<bool> DeleteAsync(Guid id);
}
