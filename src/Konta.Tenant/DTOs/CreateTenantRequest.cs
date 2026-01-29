namespace Konta.Tenant.DTOs;

/// <summary>
/// Requête pour la création d'une nouvelle entreprise (Tenant).
/// </summary>
public record CreateTenantRequest
{
    /// <summary>
    /// Nom officiel de l'entreprise.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Identifiant unique (slug) pour l'entreprise.
    /// </summary>
    public string Identifier { get; init; } = string.Empty;

    /// <summary>
    /// Secteur d'activité.
    /// </summary>
    public string? Industry { get; init; }

    /// <summary>
    /// Adresse postale.
    /// </summary>
    public string? Address { get; init; }

    /// <summary>
    /// Numéro SIRET de l'entreprise (14 chiffres).
    /// </summary>
    public string? Siret { get; init; }
}
