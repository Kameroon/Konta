using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Konta.Reporting.Services.Interfaces;
using Konta.Reporting.Data.Repositories.Interfaces;

namespace Konta.Reporting.Services.Implementations;

/// <summary>
/// Implémentation des outils d'exportation utilisant QuestPDF et ClosedXML.
/// </summary>
public class ExportService : IExportService
{
    private readonly IReportingRepository _repository;
    private readonly IKpiService _kpiService;
    private readonly ILogger<ExportService> _logger;

    public ExportService(
        IReportingRepository repository, 
        IKpiService kpiService,
        ILogger<ExportService> logger)
    {
        _repository = repository;
        _kpiService = kpiService;
        _logger = logger;
        
        // Configuration de la licence QuestPDF (Community)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Génère un PDF professionnel de synthèse.
    /// </summary>
    public async Task<byte[]> GenerateFinancialPdfAsync(Guid tenantId)
    {
        _logger.LogInformation("Génération d'un rapport PDF pour le tenant {TenantId}", tenantId);
        
        var summary = await _kpiService.GetDashboardSummaryAsync(tenantId);
        
        // Utilisation du moteur Fluent de QuestPDF
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                
                page.Header().Text("RAPPORT FINANCIER KONTA ERP")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);
                
                page.Content().Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Text($"Période : {summary.Period}").Italic();
                    col.Item().LineHorizontal(1);
                    
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c => {
                            c.Item().Text("Chiffre d'Affaires").Bold();
                            c.Item().Text($"{summary.TotalRevenue:N2} €");
                        });
                        row.RelativeItem().Column(c => {
                            c.Item().Text("Charges").Bold();
                            c.Item().Text($"{summary.TotalExpenses:N2} €");
                        });
                    });
                    
                    col.Item().PaddingTop(20).Text($"Marge brute : {summary.GrossMargin:N2} €")
                        .FontSize(16).FontColor(summary.GrossMargin > 0 ? Colors.Green.Medium : Colors.Red.Medium);
                });
                
                page.Footer().AlignCenter().Text(x => {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                });
            });
        });

        return document.GeneratePdf();
    }

    /// <summary>
    /// Génère un fichier Excel listant les flux de trésorerie.
    /// </summary>
    public async Task<byte[]> GenerateCashFlowExcelAsync(Guid tenantId)
    {
        _logger.LogInformation("Génération d'un export Excel pour le tenant {TenantId}", tenantId);
        
        var trends = await _repository.GetCashFlowTrendAsync(tenantId, 30);
        
        // Utilisation de ClosedXML
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Trésorerie");
        
        // En-têtes
        worksheet.Cell(1, 1).Value = "Date";
        worksheet.Cell(1, 2).Value = "Solde";
        worksheet.Cell(1, 3).Value = "Entrées";
        worksheet.Cell(1, 4).Value = "Sorties";
        
        var row = 2;
        foreach (var trend in trends)
        {
            worksheet.Cell(row, 1).Value = trend.Date;
            worksheet.Cell(row, 2).Value = trend.Balance;
            worksheet.Cell(row, 3).Value = trend.Inflow;
            worksheet.Cell(row, 4).Value = trend.Outflow;
            row++;
        }
        
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
