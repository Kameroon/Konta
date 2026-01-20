namespace Konta.Billing.Services.Interfaces;

public interface IStripeService
{
    Task<string> CreateCheckoutSessionAsync(Guid tenantId, string planCode, string priceId);
    Task<string> CreateCustomerPortalSessionAsync(Guid tenantId);
}
