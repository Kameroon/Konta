using Konta.Finance.Models;

namespace Konta.Finance.Data.Repositories.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id);
    Task<Account?> GetByCodeAsync(Guid tenantId, string code);
    Task<IEnumerable<Account>> GetAllByTenantIdAsync(Guid tenantId);
    Task<Guid> CreateAsync(Account account);
    Task<bool> UpdateAsync(Account account);
    Task<bool> DeleteAsync(Guid id);
}
