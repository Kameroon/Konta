namespace Konta.Reporting.Services.Interfaces;

/// <summary>
/// Service de génération d'exports de rapports financiers.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Génère un rapport de synthèse financière au format PDF.
    /// </summary>
    Task<byte[]> GenerateFinancialPdfAsync(Guid tenantId);
    
    /// <summary>
    /// Génère un export Excel des tendances de trésorerie.
    /// </summary>
    Task<byte[]> GenerateCashFlowExcelAsync(Guid tenantId);
}
