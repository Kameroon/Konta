namespace Konta.Finance.Core.Services.Interfaces;

public interface IBudgetService
{
    /// <summary>
    /// Enregistre une dépense sur un budget et vérifie les seuils d'alerte.
    /// </summary>
    Task TrackSpendingAsync(Guid tenantId, string category, decimal amount, Guid? relatedEntityId = null);
    
    /// <summary>
    /// Vérifie tous les budgets d'un tenant pour d'éventuels dépassements.
    /// </summary>
    Task CheckBudgetAlertsAsync(Guid tenantId);
}
