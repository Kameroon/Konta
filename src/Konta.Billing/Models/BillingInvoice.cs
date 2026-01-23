using Konta.Shared.Models;

namespace Konta.Billing.Models;

/// <summary>
/// Représente une facture générée pour un Tenant.
/// </summary>
public class BillingInvoice : BaseEntity
{
    /// <summary>
    /// Identifiant du tenant propriétaire.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Identifiant de la facture dans Stripe (ex: in_...).
    /// </summary>
    public string StripeInvoiceId { get; set; } = string.Empty;

    /// <summary>
    /// Numéro de facture lisible (ex: INV-2026-001).
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Montant total hors taxes.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Devise (ex: EUR, USD).
    /// </summary>
    public string Currency { get; set; } = "EUR";

    /// <summary>
    /// Statut de la facture (paid, open, void, uncollectible).
    /// </summary>
    public string Status { get; set; } = "open";

    /// <summary>
    /// URL du PDF de la facture sur Stripe ou stockage local.
    /// </summary>
    public string? PdfUrl { get; set; }

    /// <summary>
    /// Date à laquelle la facture a été payée.
    /// </summary>
    public DateTime? PaidAt { get; set; }
}
