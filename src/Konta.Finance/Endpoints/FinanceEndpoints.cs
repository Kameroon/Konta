using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Konta.Finance.Models;
using Konta.Finance.Services.Interfaces;
using Konta.Finance.Data.Repositories.Interfaces;
using Konta.Shared.Data;
using Konta.Shared.Responses;

namespace Konta.Finance.Endpoints;

public static class FinanceEndpoints
{
    public static void MapFinanceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/finance").WithTags("Finance");

        // --- Accounts ---
        group.MapGet("/accounts", async (ITenantContext tenantContext, IAccountRepository repo) =>
        {
            if (!tenantContext.TenantId.HasValue) return Results.Unauthorized();
            var accounts = await repo.GetAllByTenantIdAsync(tenantContext.TenantId.Value);
            return Results.Ok(ApiResponse<object>.Ok(accounts));
        }).RequireAuthorization();

        group.MapPost("/accounts/initialize", async (ITenantContext tenantContext, IAccountingService service) =>
        {
            if (!tenantContext.TenantId.HasValue) return Results.Unauthorized();
            await service.InitializeDefaultAccountsAsync(tenantContext.TenantId.Value);
            return Results.Ok(ApiResponse.Ok("Plan comptable initialisé."));
        }).RequireAuthorization();

        // --- Entries ---
        group.MapPost("/entries", async (JournalEntry entry, IAccountingService service) =>
        {
            try
            {
                var id = await service.PostEntryAsync(entry);
                return Results.Created($"/api/finance/entries/{id}", new { Id = id });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { Error = ex.Message });
            }
        }).RequireAuthorization();

        group.MapGet("/entries/{id}", async (Guid id, IJournalEntryRepository repo) =>
        {
            var entry = await repo.GetByIdAsync(id);
            return entry is not null ? Results.Ok(entry) : Results.NotFound();
        }).RequireAuthorization();

        // --- Reporting ---
        group.MapGet("/accounts/{accountId}/balance", async (Guid accountId, DateTime? date, IAccountingService service) =>
        {
            var balance = await service.GetAccountBalanceAsync(accountId, date);
            return Results.Ok(new { AccountId = accountId, Balance = balance });
        }).RequireAuthorization();
    }
}
