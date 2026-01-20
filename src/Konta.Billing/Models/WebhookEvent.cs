using Konta.Shared.Models;

namespace Konta.Billing.Models;

/// <summary>
/// Gardien de l'idempotence. Stocke les identifiants d'événements Stripe déjà traités.
/// </summary>
public class WebhookEvent : BaseEntity
{
    /// <summary>
    /// Identifiant unique de l'événement envoyé par Stripe (ex: evt_...).
    /// </summary>
    public string StripeEventId { get; set; } = string.Empty;

    /// <summary>
    /// Type d'événement (ex: invoice.paid).
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Date de réception.
    /// </summary>
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indique si le traitement a réussi.
    /// </summary>
    public bool ProcessedSuccessfully { get; set; }
}
