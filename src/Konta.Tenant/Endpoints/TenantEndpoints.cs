using Konta.Tenant.DTOs; // Accès aux DTOs
using Konta.Tenant.Services.Interfaces; // Accès aux services métier
using Konta.Tenant.Filters; // Utilisation du filtre de validation
using Konta.Shared.Responses; // Réponses API standard
using Microsoft.AspNetCore.Mvc; // Attributs MVC

namespace Konta.Tenant.Endpoints; // Espace de noms pour les points d'entrée

/// <summary>
/// Définition des points d'entrée API pour la gestion des entreprises (Tenants).
/// </summary>
public static class TenantEndpoints
{
    /// <summary>
    /// Enregistre les routes liées aux tenants dans l'application.
    /// </summary>
    public static void MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        // Création d'un groupe de routes /api/tenants avec un tag pour Swagger
        var group = app.MapGroup("/api/tenants")
            .WithTags("Tenants - Gestion des Entreprises");

        // POST : Création d'une nouvelle entreprise
        group.MapPost("/", async (CreateTenantRequest request, ITenantService tenantService) =>
        {
            // Appel au service pour créer l'entité et l'abonnement initial
            var response = await tenantService.CreateTenantAsync(request);
            // Retourne un succès 200 OK avec le détail du nouveau tenant
            return Results.Ok(ApiResponse<TenantResponse>.Ok(response, "La nouvelle entreprise a été enregistrée avec succès."));
        })
        .WithName("CreateTenant") // Nom unique pour l'endpoint
        .AddEndpointFilter<ValidationFilter<CreateTenantRequest>>() // Application automatique de la validation
;

        // GET : Récupération de la liste complète des entreprises
        group.MapGet("/", async (ITenantService tenantService) =>
        {
            // Récupération de tous les tenants depuis le service
            var tenants = await tenantService.GetAllTenantsAsync();
            // Retourne la liste enveloppée dans l' ApiResponse standard
            return Results.Ok(ApiResponse<IEnumerable<TenantResponse>>.Ok(tenants, "Liste des entreprises récupérée avec succès."));
        })
        .WithName("GetAllTenants")
;

        // GET : Récupération des détails d'une entreprise spécifique par son ID
        group.MapGet("/{id:guid}", async (Guid id, ITenantService tenantService) =>
        {
            // Recherche du tenant par son identifiant global unique
            var tenant = await tenantService.GetTenantAsync(id);
            // Gestion du code retour selon l'existence du tenant
            return tenant == null 
                ? Results.NotFound(ApiResponse<object>.Fail("Entreprise introuvable avec cet identifiant."))
                : Results.Ok(ApiResponse<TenantResponse>.Ok(tenant, "Détails de l'entreprise récupérés."));
        })
        .WithName("GetTenantById")
;

        // GET : Recherche d'une entreprise par son identifiant (SIRET/slug) - endpoint PUBLIC
        group.MapGet("/lookup/{identifier}", async (string identifier, ITenantService tenantService) =>
        {
            // Recherche du tenant par son identifiant unique
            var tenant = await tenantService.GetTenantByIdentifierAsync(identifier);
            // Gestion du code retour selon l'existence du tenant
            return tenant == null 
                ? Results.NotFound(ApiResponse<object>.Fail("Impossible de trouver l'entreprise. Vérifiez le SIRET."))
                : Results.Ok(ApiResponse<TenantResponse>.Ok(tenant, "Entreprise trouvée."));
        })
        .WithName("LookupTenantByIdentifier")
        .AllowAnonymous() // Endpoint public pour l'inscription
;
    }
}
