using Konta.Billing.Models;

namespace Konta.Billing.Data.Repositories.Interfaces;

public interface IStripeCustomerRepository
{
    Task<StripeCustomer?> GetByTenantIdAsync(Guid tenantId);
    Task<StripeCustomer?> GetByStripeIdAsync(string stripeCustomerId);
    Task<Guid> CreateAsync(StripeCustomer customer);
}
