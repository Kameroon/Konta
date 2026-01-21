// Importation des namespaces nécessaires
using Microsoft.AspNetCore.Http; // Pour accéder au contexte HTTP
using Konta.Shared.Data; // Pour ITenantContext
using System.Security.Claims; // Pour manipuler les claims JWT

namespace Konta.Shared.Middleware;

/// <summary>
/// Middleware responsable de l'extraction du TenantId depuis le JWT et de son injection dans le contexte de la requête.
/// Ce middleware garantit l'isolation multi-tenant en identifiant l'entreprise (tenant) de l'utilisateur connecté.
/// </summary>
public class TenantMiddleware
{
    // Délégué vers le prochain middleware dans le pipeline ASP.NET Core
    private readonly RequestDelegate _next;

    /// <summary>
    /// Constructeur du middleware. Injecte le prochain middleware du pipeline.
    /// </summary>
    /// <param name="next">Le middleware suivant à appeler après celui-ci.</param>
    public TenantMiddleware(RequestDelegate next)
    {
        _next = next; // Stockage du prochain middleware
    }

    /// <summary>
    /// Méthode principale du middleware, exécutée pour chaque requête HTTP.
    /// Extrait le TenantId du JWT et l'injecte dans le TenantContext pour toute la durée de la requête.
    /// </summary>
    /// <param name="context">Le contexte HTTP de la requête en cours.</param>
    /// <param name="tenantContext">Le contexte tenant (scoped) injecté automatiquement par DI.</param>
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // Vérifier si l'utilisateur est authentifié (a un JWT valide)
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Extraire la valeur du claim "TenantId" depuis le JWT
            var tenantIdClaim = context.User.FindFirst("TenantId")?.Value;
            
            // Tenter de convertir la chaîne en Guid (format UUID)
            if (Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                // Injecter le TenantId dans le contexte (disponible pour tous les repositories et services)
                tenantContext.TenantId = tenantId;
            }
            // Si le parsing échoue ou si le claim n'existe pas, TenantId reste null
        }
        // Si l'utilisateur n'est pas authentifié, TenantId reste null (requêtes publiques comme /login)

        // Passer la main au middleware suivant dans le pipeline
        await _next(context);
    }
}

