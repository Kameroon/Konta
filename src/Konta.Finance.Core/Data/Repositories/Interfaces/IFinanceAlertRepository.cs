using Konta.Finance.Core.Models;

namespace Konta.Finance.Core.Data.Repositories.Interfaces;

public interface IFinanceAlertRepository
{
    Task<IEnumerable<FinanceAlert>> GetUnreadByTenantIdAsync(Guid tenantId);
    Task<Guid> CreateAsync(FinanceAlert alert);
    Task<bool> MarkAsReadAsync(Guid id);
}
