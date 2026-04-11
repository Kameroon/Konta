using Konta.Billing.Services.Implementations;
using Konta.Billing.Services.Interfaces;
using Konta.Billing.Data.Repositories.Implementations;
using Konta.Billing.Data.Repositories.Interfaces;
using Konta.Shared.Data;
using Konta.Shared.Extensions;
using Konta.Shared.Middleware;
using Konta.Billing.Data;

namespace Konta.Billing.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBillingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Repositories
        services.AddScoped<IStripeCustomerRepository, StripeCustomerRepository>();
        services.AddScoped<IBillingInvoiceRepository, BillingInvoiceRepository>();
        services.AddScoped<IWebhookEventRepository, WebhookEventRepository>();
        services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();

        // Initializer
        services.AddScoped<DatabaseInitializer>();

        return services;
    }

    public static IServiceCollection AddBillingServices(this IServiceCollection services)
    {
        // Domain Services
        services.AddScoped<IStripeService, StripeService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<WebhookHandler>();

        return services;
    }
}
