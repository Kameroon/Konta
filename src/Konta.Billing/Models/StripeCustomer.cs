using Konta.Shared.Models;

namespace Konta.Billing.Models;

/// <summary>
/// Mappe un Tenant d'entreprise à un identifiant client Stripe.
/// </summary>
public class StripeCustomer : BaseEntity
{
    /// <summary>
    /// Identifiant unique du Tenant propriétaire.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Identifiant du client dans Stripe (ex: cus_...).
    /// </summary>
    public string StripeCustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Email associé au compte Stripe.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
