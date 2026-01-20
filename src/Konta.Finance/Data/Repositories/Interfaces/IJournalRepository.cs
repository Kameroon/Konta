using Konta.Finance.Models;

namespace Konta.Finance.Data.Repositories.Interfaces;

public interface IJournalRepository
{
    Task<Journal?> GetByIdAsync(Guid id);
    Task<Journal?> GetByCodeAsync(Guid tenantId, string code);
    Task<IEnumerable<Journal>> GetAllByTenantIdAsync(Guid tenantId);
    Task<Guid> CreateAsync(Journal journal);
    Task<bool> UpdateAsync(Journal journal);
}
