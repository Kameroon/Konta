using Konta.Reporting.Models.Analytics;

namespace Konta.Reporting.Data.Repositories.Interfaces;

/// <summary>
/// Interface pour l'agrégation de données analytiques.
/// </summary>
public interface IReportingRepository
{
    /// <summary>
    /// Récupère le résumé financier d'un tenant pour une période.
    /// </summary>
    Task<FinancialSummary> GetFinancialSummaryAsync(Guid tenantId, string period);
    
    /// <summary>
    /// Calcule l'évolution de la trésorerie sur les X derniers jours.
    /// </summary>
    Task<IEnumerable<CashFlowTrend>> GetCashFlowTrendAsync(Guid tenantId, int days);
    
    /// <summary>
    /// Récupère la répartition du chiffre d'affaires par catégorie.
    /// </summary>
    Task<IEnumerable<DashboardKpi>> GetRevenueBreakdownAsync(Guid tenantId);

    /// <summary>
    /// Récupère un résumé complet pour le Dashboard (Stats globales ou par tenant).
    /// </summary>
    Task<DashboardSummary> GetFullDashboardSummaryAsync(Guid? tenantId);
}
