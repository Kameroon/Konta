using Konta.Shared.Models;

namespace Konta.Reporting.Models.Analytics;

/// <summary>
/// Représente un résumé financier agrégé pour un tableau de bord.
/// </summary>
public class FinancialSummary : BaseEntity
{
    /// <summary> Identifiant du locataire (multi-tenant) </summary>
    public Guid TenantId { get; set; }
    
    /// <summary> Chiffre d'affaires total (Ventes validées) </summary>
    public decimal TotalRevenue { get; set; }
    
    /// <summary> Total des charges (Achats validés) </summary>
    public decimal TotalExpenses { get; set; }
    
    /// <summary> Marge brute calculée </summary>
    public decimal GrossMargin => TotalRevenue - TotalExpenses;
    
    /// <summary> Ratio de rentabilité en pourcentage </summary>
    public decimal ProfitabilityPercentage => TotalRevenue > 0 ? (GrossMargin / TotalRevenue) * 100 : 0;
    
    /// <summary> Période concernée (ex: "2024-01") </summary>
    public string Period { get; set; } = string.Empty;
}
