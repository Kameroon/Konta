using Konta.Billing.Models;

namespace Konta.Billing.Data.Repositories.Interfaces;

public interface IWebhookEventRepository
{
    Task<bool> ExistsAsync(string stripeEventId);
    Task AddAsync(WebhookEvent @event);
}
