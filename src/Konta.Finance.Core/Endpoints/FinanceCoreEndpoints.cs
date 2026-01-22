using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Konta.Finance.Core.Models;
using Konta.Finance.Core.Data.Repositories.Interfaces;
using Konta.Finance.Core.Services.Interfaces;
using Konta.Shared.Data;
using Konta.Shared.Responses;

namespace Konta.Finance.Core.Endpoints;

/// <summary>
/// Définition des points d'entrée API pour le cœur financier.
/// </summary>
public static class FinanceCoreEndpoints
{
    public static void MapFinanceCoreEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/finance-core").WithTags("Finance Core");

        // --- GESTION DES TIERS (Clients & Fournisseurs) ---
        
        // Créer un nouveau profil tiers
        group.MapPost("/tiers", async (Tier tier, ITierRepository repo) =>
        {
            var id = await repo.CreateAsync(tier);
            return Results.Created($"/api/finance-core/tiers/{id}", ApiResponse<object>.Ok(new { Id = id }));
        }).RequireAuthorization();

        // Récupérer les tiers d'une entreprise
        group.MapGet("/tiers", async (ITenantContext tenantContext, TierType? type, ITierRepository repo) => 
        {
            if (!tenantContext.TenantId.HasValue) return Results.Unauthorized();
            var tiers = await repo.GetByTenantIdAsync(tenantContext.TenantId.Value, type);
            return Results.Ok(ApiResponse<object>.Ok(tiers));
        }).RequireAuthorization();

        // --- GESTION BUDGÉTAIRE ---
        
        // Définir une nouvelle enveloppe budgétaire
        group.MapPost("/budgets", async (Budget budget, IBudgetRepository repo) =>
        {
            var id = await repo.CreateAsync(budget);
            return Results.Created($"/api/finance-core/budgets/{id}", ApiResponse<object>.Ok(new { Id = id }));
        }).RequireAuthorization();

        // Consulter les budgets actifs et leur état de consommation
        group.MapGet("/budgets", async (ITenantContext tenantContext, IBudgetRepository repo) =>
        {
            if (!tenantContext.TenantId.HasValue) return Results.Unauthorized();
            var budgets = await repo.GetCurrentBudgetsAsync(tenantContext.TenantId.Value, DateTime.UtcNow);
            return Results.Ok(ApiResponse<object>.Ok(budgets));
        }).RequireAuthorization();

        // --- FACTURATION OPÉRATIONNELLE ---
        
        // Enregistrer une facture (Achat ou Vente)
        group.MapPost("/invoices", async (BusinessInvoice invoice, IInvoiceRepository repo, IBudgetService budgetService) =>
        {
            // Attribution automatique de l'ID si manquant
            if (invoice.Id == Guid.Empty) invoice.Id = Guid.NewGuid();
            
            var id = await repo.CreateAsync(invoice);
            
            // Impact budgétaire automatique pour les factures fournisseur validées
            if (invoice.IsPurchase && invoice.Status == InvoiceStatus.Validated)
            {
                // Dans le cadre du MVP, nous utilisons une catégorie par défaut "Général"
                await budgetService.TrackSpendingAsync(invoice.TenantId, "Général", invoice.AmountHt, id);
            }

            return Results.Created($"/api/finance-core/invoices/{id}", ApiResponse<object>.Ok(new { Id = id }));
        }).RequireAuthorization();

        // --- OPÉRATIONS DE TRÉSORERIE ---
        
        // Enregistrer un flux d'argent (Encaissement ou Décaissement)
        group.MapPost("/treasury/movement", async (Guid accountId, decimal amount, bool isIncome, ITreasuryService service) =>
        {
            await service.RegisterMovementAsync(accountId, amount, isIncome);
            return Results.Ok(ApiResponse.Ok("Mouvement de trésorerie validé et enregistré."));
        }).RequireAuthorization();

        // --- ALERTES ET SURVEILLANCE ---
        
        // Récupérer les alertes financières non lues
        group.MapGet("/alerts", async (ITenantContext tenantContext, IFinanceAlertRepository repo) =>
        {
            if (!tenantContext.TenantId.HasValue) return Results.Unauthorized();
            var alerts = await repo.GetUnreadByTenantIdAsync(tenantContext.TenantId.Value);
            return Results.Ok(ApiResponse<object>.Ok(alerts));
        }).RequireAuthorization();

        // Marquer une alerte comme traitée
        group.MapPatch("/alerts/{id}/read", async (Guid id, IFinanceAlertRepository repo) =>
        {
            await repo.MarkAsReadAsync(id);
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
