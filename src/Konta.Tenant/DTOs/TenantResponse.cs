namespace Konta.Tenant.DTOs;

/// <summary>
/// Réponse contenant les informations d'une entreprise.
/// </summary>
public record TenantResponse
{
    /// <summary>
    /// Identifiant unique global de l'entreprise.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Nom de l'entreprise.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Identifiant (slug) unique de l'entreprise.
    /// </summary>
    public string Identifier { get; init; } = string.Empty;

    /// <summary>
    /// Secteur d'activité.
    /// </summary>
    public string? Industry { get; init; }

    /// <summary>
    /// Date de création du compte.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Indique si le compte de l'entreprise est actif.
    /// </summary>
    public bool IsActive { get; init; }
}
