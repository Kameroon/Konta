using Konta.Billing.Models;

namespace Konta.Billing.Data.Repositories.Interfaces;

public interface IBillingInvoiceRepository
{
    Task<IEnumerable<BillingInvoice>> GetByTenantIdAsync(Guid tenantId);
    Task<BillingInvoice?> GetByStripeIdAsync(string stripeInvoiceId);
    Task<Guid> CreateAsync(BillingInvoice invoice);
    Task UpdateStatusAsync(string stripeInvoiceId, string status, DateTime? paidAt = null);
}
