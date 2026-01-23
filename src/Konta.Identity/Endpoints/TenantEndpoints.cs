using Konta.Identity.DTOs;
using Konta.Identity.Filters;
using Konta.Identity.Services.Interfaces;
using Konta.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Konta.Identity.Endpoints;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenants")
            .WithTags("Tenants");

        group.MapGet("/lookup/{siret}", async (string siret, ICompanyRegistryService registryService) =>
        {
            var result = await registryService.LookupBySiretAsync(siret);
            return result == null 
                ? Results.NotFound(ApiResponse<object>.Fail("Aucune entreprise trouvée pour ce SIRET."))
                : Results.Ok(ApiResponse<CompanyRegistrationResult>.Ok(result));
        })
        .AllowAnonymous()
        .WithName("LookupCompany")
        .Produces<ApiResponse<CompanyRegistrationResult>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        group.MapGet("/", async (ITenantService tenantService) =>
        {
            var tenants = await tenantService.GetAllTenantsAsync();
            return Results.Ok(ApiResponse<IEnumerable<TenantResponse>>.Ok(tenants));
        })
        .RequireAuthorization()
        .WithName("GetAllTenants")
        .Produces<ApiResponse<IEnumerable<TenantResponse>>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, ITenantService tenantService) =>
        {
            var tenant = await tenantService.GetTenantByIdAsync(id);
            return tenant == null 
                ? Results.NotFound(ApiResponse<object>.Fail("Tenant non trouvé"))
                : Results.Ok(ApiResponse<TenantResponse>.Ok(tenant));
        })
        .RequireAuthorization()
        .WithName("GetTenantById")
        .Produces<ApiResponse<TenantResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", async (Guid id, UpdateTenantRequest request, ITenantService tenantService) =>
        {
            var success = await tenantService.UpdateTenantAsync(id, request);
            return success 
                ? Results.Ok(ApiResponse.Ok("Tenant mis à jour avec succès"))
                : Results.NotFound(ApiResponse<object>.Fail("Tenant non trouvé"));
        })
        .RequireAuthorization()
        .WithName("UpdateTenant")
        .AddEndpointFilter<ValidationFilter<UpdateTenantRequest>>()
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}/plan", async (Guid id, [FromBody] string plan, ITenantService tenantService) =>
        {
            var success = await tenantService.UpdatePlanAsync(id, plan);
            return success 
                ? Results.Ok(ApiResponse.Ok("Plan mis à jour avec succès"))
                : Results.NotFound(ApiResponse<object>.Fail("Tenant non trouvé"));
        })
        .RequireAuthorization()
        .WithName("UpdateTenantPlan")
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, ITenantService tenantService) =>
        {
            var success = await tenantService.DeleteTenantAsync(id);
            return success 
                ? Results.Ok(ApiResponse.Ok("Tenant supprimé avec succès"))
                : Results.NotFound(ApiResponse<object>.Fail("Tenant non trouvé"));
        })
        .RequireAuthorization()
        .WithName("DeleteTenant")
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}
