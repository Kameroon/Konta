namespace Konta.Identity.DTOs;

/// <summary>
/// Requête pour la mise à jour des informations d'un tenant.
/// </summary>
public record UpdateTenantRequest
{
    /// <summary>
    /// Nouveau nom de l'entreprise.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Nouveau plan d'abonnement.
    /// </summary>
    public string Plan { get; init; } = "Free";
}
