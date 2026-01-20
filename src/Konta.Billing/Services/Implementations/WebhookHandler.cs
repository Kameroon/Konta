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
    private readonly ILogger<WebhookHandler> _logger;
    private readonly IConfiguration _configuration;

    public WebhookHandler(
        IWebhookEventRepository eventRepository,
        IBillingInvoiceRepository invoiceRepository,
        ILogger<WebhookHandler> logger,
        IConfiguration configuration)
    {
        _eventRepository = eventRepository;
        _invoiceRepository = invoiceRepository;
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

    private async Task<bool> HandleInvoicePaidAsync(Invoice? invoice)
    {
        if (invoice == null) return false;

        _logger.LogInformation("Traitement du paiement réussi pour la facture {Id}", invoice.Id);

        // Mise à jour de la facture locale
        await _invoiceRepository.UpdateStatusAsync(invoice.Id, "paid", DateTime.UtcNow);

        // TODO: Déclencher un événement (RabbitMQ ou autre) pour informer Konta.Tenant
        // afin de débloquer les accès si nécessaire.
        
        return true;
    }

    private async Task<bool> HandleSubscriptionDeletedAsync(Subscription? subscription)
    {
        if (subscription == null) return false;

        _logger.LogWarning("L'abonnement Stripe {Id} a été supprimé.", subscription.Id);

        // TODO: Désactiver les accès du tenant dans Konta.Tenant
        
        return true;
    }
}
