using System.ComponentModel.DataAnnotations;

namespace Konta.Identity.DTOs;

/// <summary>
/// Requête pour l'assignation d'une permission.
/// </summary>
public record AssignPermissionRequest
{
    /// <summary>
    /// Identifiant de la permission à assigner.
    /// </summary>
    [Required(ErrorMessage = "L'identifiant de la permission est obligatoire.")]
    public Guid PermissionId { get; init; }
}
