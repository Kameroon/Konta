using Konta.Finance.Core.Models;

namespace Konta.Finance.Core.Data.Repositories.Interfaces;

public interface IBudgetRepository
{
    Task<Budget?> GetByIdAsync(Guid id);
    Task<IEnumerable<Budget>> GetCurrentBudgetsAsync(Guid tenantId, DateTime date);
    Task<Guid> CreateAsync(Budget budget);
    Task<bool> UpdateSpentAmountAsync(Guid id, decimal amount);
}
