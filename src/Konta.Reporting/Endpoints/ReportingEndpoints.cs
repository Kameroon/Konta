using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Konta.Reporting.Services.Interfaces;
using Konta.Shared.Responses;
using Konta.Shared.Data;

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
        group.MapGet("/dashboard/summary", async (ITenantContext tenantContext, IKpiService service) =>
        {
            if (!tenantContext.TenantId.HasValue) return Results.Unauthorized();
            var summary = await service.GetDashboardSummaryAsync(tenantContext.TenantId.Value);
            return Results.Ok(ApiResponse<object>.Ok(summary));
        }).RequireAuthorization();

        // Obtenir la liste des KPIs principaux
        group.MapGet("/kpi/main", async (ITenantContext tenantContext, IKpiService service) =>
        {
            if (!tenantContext.TenantId.HasValue) return Results.Unauthorized();
            var kpis = await service.GetMainKpisAsync(tenantContext.TenantId.Value);
            return Results.Ok(ApiResponse<object>.Ok(kpis));
        }).RequireAuthorization();
        
        // Obtenir l'évolution de la trésorerie (Trend)
        group.MapGet("/dashboard/cashflow", async (int? days, ITenantContext tenantContext, IKpiService service) =>
        {
            if (!tenantContext.TenantId.HasValue) return Results.Unauthorized();
            var trend = await service.GetCashFlowTrendAsync(tenantContext.TenantId.Value, days ?? 30);
            return Results.Ok(ApiResponse<object>.Ok(trend));
        }).RequireAuthorization();

        // --- EXPORTS ---
        
        // Générer et télécharger le PDF de synthèse
        group.MapGet("/export/pdf", async (ITenantContext tenantContext, IExportService service) =>
        {
            if (!tenantContext.TenantId.HasValue) return Results.Unauthorized();
            var pdf = await service.GenerateFinancialPdfAsync(tenantContext.TenantId.Value);
            return Results.File(pdf, "application/pdf", $"Rapport_{DateTime.Now:yyyyMMdd}.pdf");
        }).RequireAuthorization();

        // Générer et télécharger l'export Excel de trésorerie
        group.MapGet("/export/excel", async (ITenantContext tenantContext, IExportService service) =>
            {
                if (!tenantContext.TenantId.HasValue) return Results.Unauthorized();
                var excel = await service.GenerateCashFlowExcelAsync(tenantContext.TenantId.Value);
                return Results.File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Tresorerie_{DateTime.Now:yyyyMMdd}.xlsx");
            }).RequireAuthorization();
    }
}
