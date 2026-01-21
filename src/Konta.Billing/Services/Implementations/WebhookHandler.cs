using Stripe;
using Konta.Billing.Data.Repositories.Interfaces;
using Konta.Billing.Models;

namespace Konta.Billing.Services.Implementations;

/// <summary>
/// Gestionnaire des événements Webhook Stripe avec garantie d'idempotence.
/// </summary>
public class WebhookHandler
{
    private readonly IWebhookEventRepository _eventRepository;
    private readonly IBillingInvoiceRepository _invoiceRepository;
    private readonly IStripeCustomerRepository _customerRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookHandler> _logger;
    private readonly IConfiguration _configuration;

    public WebhookHandler(
        IWebhookEventRepository eventRepository,
        IBillingInvoiceRepository invoiceRepository,
        IStripeCustomerRepository customerRepository,
        IHttpClientFactory httpClientFactory,
        ILogger<WebhookHandler> logger,
        IConfiguration configuration)
    {
        _eventRepository = eventRepository;
        _invoiceRepository = invoiceRepository;
        _customerRepository = customerRepository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task HandleAsync(string json, string signature)
    {
        try
        {
            // 1. Vérification de la signature
            var stripeEvent = EventUtility.ConstructEvent(
                json, 
                signature, 
                _configuration["Stripe:WebhookSecret"]
            );

            _logger.LogInformation("Webhook reçu : {Id} (Type: {Type})", stripeEvent.Id, stripeEvent.Type);

            // 2. Vérification de l'idempotence (évite les doublons)
            if (await _eventRepository.ExistsAsync(stripeEvent.Id))
            {
                _logger.LogWarning("Événement {Id} déjà traité. Ignoré.", stripeEvent.Id);
                return;
            }

            // 3. Routage de l'événement
            bool success = stripeEvent.Type switch
            {
                Events.InvoicePaid => await HandleInvoicePaidAsync(stripeEvent.Data.Object as Invoice),
                Events.CustomerSubscriptionDeleted => await HandleSubscriptionDeletedAsync(stripeEvent.Data.Object as Subscription),
                _ => true // On ignore les autres types
            };

            // 4. Enregistrement du succès du traitement
            await _eventRepository.AddAsync(new WebhookEvent
            {
                StripeEventId = stripeEvent.Id,
                EventType = stripeEvent.Type,
                ProcessedSuccessfully = success
            });
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement du webhook Stripe");
            throw;
        }
    }

    /// <summary>
    /// Gère l'événement de paiement réussi d'une facture Stripe.
    /// Met à jour le statut local et active l'accès du tenant.
    /// </summary>
    private async Task<bool> HandleInvoicePaidAsync(Invoice? invoice)
    {
        if (invoice == null) return false;

        _logger.LogInformation("Traitement du paiement réussi pour la facture Stripe {InvoiceId}", invoice.Id);

        try
        {
            // 1. Mise à jour de la facture locale dans Konta.Billing
            await _invoiceRepository.UpdateStatusAsync(invoice.Id, "paid", DateTime.UtcNow);
            _logger.LogInformation("Statut de la facture {InvoiceId} mis à jour en base de données", invoice.Id);

            // 2. Récupérer le TenantId associé au client Stripe
            var stripeCustomer = await _customerRepository.GetByStripeIdAsync(invoice.CustomerId);
            if (stripeCustomer == null)
            {
                _logger.LogError("Client Stripe {CustomerId} introuvable dans la base Konta.Billing", invoice.CustomerId);
                return false;
            }

            // 3. Appel HTTP vers Konta.Tenant pour activer l'accès
            // NOTE: Dans une architecture event-driven mature, ceci serait un événement publié sur RabbitMQ/Kafka
            // Exemple : await _messageBus.PublishAsync(new TenantAccessGrantedEvent { TenantId = stripeCustomer.TenantId });
            
            var tenantServiceUrl = _configuration["Services:Tenant:BaseUrl"] ?? "https://localhost:5002";
            var httpClient = _httpClientFactory.CreateClient();
            
            var activateRequest = new
            {
                TenantId = stripeCustomer.TenantId,
                Reason = "Paiement reçu",
                InvoiceId = invoice.Id,
                Amount = invoice.AmountPaid / 100.0 // Conversion centimes → euros
            };

            var response = await httpClient.PostAsJsonAsync(
                $"{tenantServiceUrl}/api/tenant/activate-access", 
                activateRequest
            );

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Accès activé avec succès pour le tenant {TenantId} suite au paiement de {Amount}€", 
                    stripeCustomer.TenantId, activateRequest.Amount);
                return true;
            }
            else
            {
                _logger.LogError("Échec de l'activation de l'accès pour le tenant {TenantId}. Status: {StatusCode}", 
                    stripeCustomer.TenantId, response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement de la facture payée {InvoiceId}", invoice.Id);
            return false;
        }
    }

    /// <summary>
    /// Gère l'événement de suppression d'un abonnement Stripe.
    /// Désactive l'accès du tenant et log l'événement pour audit.
    /// </summary>
    private async Task<bool> HandleSubscriptionDeletedAsync(Subscription? subscription)
    {
        if (subscription == null) return false;

        _logger.LogWarning("L'abonnement Stripe {SubscriptionId} a été supprimé (Client: {CustomerId})", 
            subscription.Id, subscription.CustomerId);

        try
        {
            // 1. Récupérer le TenantId associé au client Stripe
            var stripeCustomer = await _customerRepository.GetByStripeIdAsync(subscription.CustomerId);
            if (stripeCustomer == null)
            {
                _logger.LogError("Client Stripe {CustomerId} introuvable dans la base Konta.Billing", subscription.CustomerId);
                return false;
            }

            // 2. Appel HTTP vers Konta.Tenant pour désactiver l'accès
            // NOTE: Dans une architecture event-driven, ceci serait un événement publié
            // Exemple : await _messageBus.PublishAsync(new TenantAccessRevokedEvent { TenantId = stripeCustomer.TenantId });
            
            var tenantServiceUrl = _configuration["Services:Tenant:BaseUrl"] ?? "https://localhost:5002";
            var httpClient = _httpClientFactory.CreateClient();
            
            var deactivateRequest = new
            {
                TenantId = stripeCustomer.TenantId,
                Reason = "Abonnement supprimé",
                SubscriptionId = subscription.Id,
                CanceledAt = subscription.CanceledAt ?? DateTime.UtcNow
            };

            var response = await httpClient.PostAsJsonAsync(
                $"{tenantServiceUrl}/api/tenant/deactivate-access", 
                deactivateRequest
            );

            if (response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Accès désactivé avec succès pour le tenant {TenantId} suite à la suppression de l'abonnement", 
                    stripeCustomer.TenantId);
                return true;
            }
            else
            {
                _logger.LogError("Échec de la désactivation de l'accès pour le tenant {TenantId}. Status: {StatusCode}", 
                    stripeCustomer.TenantId, response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement de la suppression de l'abonnement {SubscriptionId}", subscription.Id);
            return false;
        }
    }
}
