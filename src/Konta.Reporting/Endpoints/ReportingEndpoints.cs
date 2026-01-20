using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Konta.Reporting.Services.Interfaces;

namespace Konta.Reporting.Endpoints;

/// <summary>
/// Points d'entrée pour l'analyse financière et les exports.
/// </summary>
public static class ReportingEndpoints
{
    public static void MapReportingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reporting").WithTags("Reporting & Dashboards");

        // --- DASHBOARDS ---
        
        // Obtenir le résumé financier du tableau de bord (Caché)
        group.MapGet("/dashboard/summary", async (Guid tenantId, IKpiService service) =>
        {
            var summary = await service.GetDashboardSummaryAsync(tenantId);
            return Results.Ok(summary);
        }).RequireAuthorization();

        // Obtenir la liste des KPIs principaux
        group.MapGet("/kpi/main", async (Guid tenantId, IKpiService service) =>
        {
            var kpis = await service.GetMainKpisAsync(tenantId);
            return Results.Ok(kpis);
        }).RequireAuthorization();

        // --- EXPORTS ---
        
        // Générer et télécharger le PDF de synthèse
        group.MapGet("/export/pdf", async (Guid tenantId, IExportService service) =>
        {
            var pdf = await service.GenerateFinancialPdfAsync(tenantId);
            return Results.File(pdf, "application/pdf", $"Rapport_{DateTime.Now:yyyyMMdd}.pdf");
        }).RequireAuthorization();

        // Générer et télécharger l'export Excel de trésorerie
        group.MapGet("/export/excel", async (Guid tenantId, IExportService service) =>
            {
                var excel = await service.GenerateCashFlowExcelAsync(tenantId);
                return Results.File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Tresorerie_{DateTime.Now:yyyyMMdd}.xlsx");
            }).RequireAuthorization();
    }
}
