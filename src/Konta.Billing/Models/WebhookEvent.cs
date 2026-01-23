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
    /// Corps complet de l'événement au format JSON.
    /// </summary>
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// Date de prise en compte par le système.
    /// </summary>
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
