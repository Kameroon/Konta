using FluentValidation; // Utilisation de FluentValidation
using Konta.Shared.Responses; // Réponses standardisées

namespace Konta.Tenant.Filters; // Espace de noms pour les filtres

/// <summary>
/// Filtre générique permettant de valider automatiquement les requêtes entrantes.
/// </summary>
/// <typeparam name="T">Le type du DTO à valider.</typeparam>
public class ValidationFilter<T> : IEndpointFilter
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Récupération du validateur correspondant injecté dans les services
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();

        if (validator is not null)
        {
            // Recherche de l'argument correspondant au type T dans la requête
            var argument = context.Arguments.OfType<T>().FirstOrDefault();
            if (argument is not null)
            {
                // Exécution de la validation asynchrone
                var validationResult = await validator.ValidateAsync(argument);
                
                // Si la validation échoue
                if (!validationResult.IsValid)
                {
                    // Extraction des messages d'erreur
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    
                    // Retour d'une réponse 400 Bad Request avec les détails des erreurs
                    return Results.BadRequest(ApiResponse.Fail("Erreur de validation", errors));
                }
            }
        }

        // Passage à l'étape suivante du pipeline (le handler de l'endpoint)
        return await next(context);
    }
}
