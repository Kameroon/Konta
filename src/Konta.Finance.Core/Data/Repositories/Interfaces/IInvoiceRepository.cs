using Konta.Finance.Core.Models;

namespace Konta.Finance.Core.Data.Repositories.Interfaces;

public interface IInvoiceRepository
{
    Task<BusinessInvoice?> GetByIdAsync(Guid id);
    Task<IEnumerable<BusinessInvoice>> GetByTenantIdAsync(Guid tenantId, bool? isPurchase = null, InvoiceStatus? status = null);
    Task<Guid> CreateAsync(BusinessInvoice invoice);
    Task<bool> UpdateStatusAsync(Guid id, InvoiceStatus status);
}
