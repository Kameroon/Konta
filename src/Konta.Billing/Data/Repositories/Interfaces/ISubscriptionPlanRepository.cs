using Konta.Billing.Models;

namespace Konta.Billing.Data.Repositories.Interfaces;

/// <summary>
/// Interface pour l'accès aux données des plans d'abonnement.
/// </summary>
public interface ISubscriptionPlanRepository
{
    /// <summary>
    /// Récupère tous les plans d'abonnement actifs.
    /// </summary>
    Task<IEnumerable<SubscriptionPlan>> GetAllActiveAsync();

    /// <summary>
    /// Récupère un plan par son code technique.
    /// </summary>
    Task<SubscriptionPlan?> GetByCodeAsync(string code);
}
