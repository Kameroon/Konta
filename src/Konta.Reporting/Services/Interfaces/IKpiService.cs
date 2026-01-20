using Konta.Reporting.Models.Analytics;

namespace Konta.Reporting.Services.Interfaces;

/// <summary>
/// Service de calcul des indicateurs de performance financiers.
/// </summary>
public interface IKpiService
{
    /// <summary>
    /// Obtient le résumé du dashboard avec gestion du cache.
    /// </summary>
    Task<FinancialSummary> GetDashboardSummaryAsync(Guid tenantId);
    
    /// <summary>
    /// Obtient les KPIs principaux (CA, Marge, Trésorerie).
    /// </summary>
    Task<IEnumerable<DashboardKpi>> GetMainKpisAsync(Guid tenantId);
}
