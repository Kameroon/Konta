namespace Konta.Identity.DTOs;

/// <summary>
/// Réponse contenant les informations d'un tenant (entreprise).
/// </summary>
public record TenantResponse
{
    /// <summary>
    /// Identifiant unique de l'entreprise.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Nom de l'entreprise.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Plan d'abonnement actuel.
    /// </summary>
    public string Plan { get; init; } = "Free";

    /// <summary>
    /// Numéro SIRET de l'entreprise.
    /// </summary>
    public string? Siret { get; init; }

    /// <summary>
    /// Secteur d'activité.
    /// </summary>
    public string? Industry { get; init; }

    /// <summary>
    /// Adresse du siège social.
    /// </summary>
    public string? Address { get; init; }

    /// <summary>
    /// Date de création de l'enregistrement.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Date de la dernière mise à jour.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }
}
