using Konta.Finance.Core.Models;

namespace Konta.Finance.Core.Data.Repositories.Interfaces;

public interface ITreasuryRepository
{
    Task<TreasuryAccount?> GetByIdAsync(Guid id);
    Task<IEnumerable<TreasuryAccount>> GetByTenantIdAsync(Guid tenantId);
    Task<bool> UpdateBalanceAsync(Guid id, decimal newBalance);
}
