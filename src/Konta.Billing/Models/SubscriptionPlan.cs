using Konta.Shared.Models;

namespace Konta.Billing.Models;

/// <summary>
/// Définition d'un plan d'abonnement SaaS.
/// </summary>
public class SubscriptionPlan : BaseEntity
{
    /// <summary>
    /// Code technique unique (ex: 'basic', 'premium').
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Nom convivial du plan.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description marketing du plan.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Prix mensuel.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Devise (ex: EUR).
    /// </summary>
    public string Currency { get; set; } = "EUR";

    /// <summary>
    /// Intervalle de facturation (month, year).
    /// </summary>
    public string Interval { get; set; } = "month";

    /// <summary>
    /// Limite d'utilisateurs.
    /// </summary>
    public int? MaxUsers { get; set; }

    /// <summary>
    /// Limite de stockage en Go.
    /// </summary>
    public int? StorageGb { get; set; }

    /// <summary>
    /// Indique si le support prioritaire est inclus.
    /// </summary>
    public bool HasPrioritySupport { get; set; }

    /// <summary>
    /// Indique si l'accès API est inclus.
    /// </summary>
    public bool HasApiAccess { get; set; }

    /// <summary>
    /// Liste des modules inclus au format JSONB.
    /// </summary>
    public string? Modules { get; set; }

    /// <summary>
    /// Liste des caractéristiques au format JSONB (pour affichage frontend).
    /// </summary>
    public string? Features { get; set; }
}
