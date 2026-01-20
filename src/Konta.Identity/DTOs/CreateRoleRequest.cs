using System.ComponentModel.DataAnnotations;

namespace Konta.Identity.DTOs;

/// <summary>
/// Requête pour la création d'un nouveau rôle.
/// </summary>
public record CreateRoleRequest
{
    /// <summary>
    /// Nom unique du rôle au sein du tenant.
    /// </summary>
    [Required(ErrorMessage = "Le nom du rôle est obligatoire.")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description des droits associés à ce rôle.
    /// </summary>
    public string Description { get; init; } = string.Empty;
}
