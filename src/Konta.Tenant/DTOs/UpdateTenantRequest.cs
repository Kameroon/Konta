namespace Konta.Tenant.DTOs;

/// <summary>
/// Requête pour la mise à jour d'une entreprise existante.
/// </summary>
public record UpdateTenantRequest
{
    /// <summary>
    /// Nouveau nom de l'entreprise.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Nouveau secteur d'activité.
    /// </summary>
    public string? Industry { get; init; }

    /// <summary>
    /// Nouvelle adresse postale.
    /// </summary>
    public string? Address { get; init; }

    /// <summary>
    /// Nouveau numéro SIRET.
    /// </summary>
    public string? Siret { get; init; }

    /// <summary>
    /// Nouveau plan d'abonnement.
    /// </summary>
    public string Plan { get; init; } = "Free";

    /// <summary>
    /// Statut d'activation.
    /// </summary>
    public bool IsActive { get; init; } = true;
}
