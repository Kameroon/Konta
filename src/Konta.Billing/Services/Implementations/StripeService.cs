using Stripe;
using Stripe.Checkout;
using Microsoft.Extensions.Options;
using Konta.Billing.Data.Repositories.Interfaces;
using Konta.Billing.Models;
using Konta.Billing.Services.Interfaces;

namespace Konta.Billing.Services.Implementations;

/// <summary>
/// Service gérant les interactions avec l'API Stripe.
/// </summary>
public class StripeService : IStripeService
{
    private readonly IStripeCustomerRepository _customerRepository;
    private readonly ILogger<StripeService> _logger;
    private readonly IConfiguration _configuration;

    public StripeService(
        IStripeCustomerRepository customerRepository,
        ILogger<StripeService> logger,
        IConfiguration configuration)
    {
        _customerRepository = customerRepository;
        _logger = logger;
        _configuration = configuration;
        
        // Initialisation de la clé API Stripe
        StripeConfiguration.ApiKey = _configuration["Stripe:ApiKey"];
    }

    /// <inheritdoc />
    public async Task<string> CreateCheckoutSessionAsync(Guid tenantId, string planCode, string priceId)
    {
        _logger.LogInformation("Création d'une session de paiement pour le tenant {TenantId} (Plan: {PlanCode})", tenantId, planCode);

        // 1. Récupérer ou créer le client Stripe localement
        var customer = await _customerRepository.GetByTenantIdAsync(tenantId);
        string stripeCustomerId;

        if (customer == null)
        {
            _logger.LogDebug("Création d'un nouveau client Stripe pour le tenant {TenantId}", tenantId);
            var customerService = new CustomerService();
            var stripeCustomer = await customerService.CreateAsync(new CustomerCreateOptions
            {
                Metadata = new Dictionary<string, string> { { "TenantId", tenantId.ToString() } }
            });

            stripeCustomerId = stripeCustomer.Id;
            await _customerRepository.CreateAsync(new StripeCustomer
            {
                TenantId = tenantId,
                StripeCustomerId = stripeCustomerId,
                Email = stripeCustomer.Email ?? ""
            });
        }
        else
        {
            stripeCustomerId = customer.StripeCustomerId;
        }

        // 2. Créer la session Checkout
        var options = new SessionCreateOptions
        {
            Customer = stripeCustomerId,
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1,
                },
            },
            Mode = "subscription",
            SuccessUrl = _configuration["Stripe:SuccessUrl"] + "?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = _configuration["Stripe:CancelUrl"],
            SubscriptionData = new SessionSubscriptionDataOptions
            {
                Metadata = new Dictionary<string, string> 
                { 
                    { "TenantId", tenantId.ToString() },
                    { "PlanCode", planCode }
                }
            }
        };

        var service = new SessionService();
        Session session = await service.CreateAsync(options);

        _logger.LogInformation("Session de paiement créée : {SessionId}", session.Id);
        return session.Url;
    }

    /// <inheritdoc />
    public async Task<string> CreateCustomerPortalSessionAsync(Guid tenantId)
    {
        var customer = await _customerRepository.GetByTenantIdAsync(tenantId);
        if (customer == null) throw new InvalidOperationException("Client Stripe introuvable.");

        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = customer.StripeCustomerId,
            ReturnUrl = _configuration["Stripe:PortalReturnUrl"],
        };

        var service = new Stripe.BillingPortal.SessionService();
        var session = await service.CreateAsync(options);

        return session.Url;
    }
}
