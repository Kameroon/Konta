namespace Konta.Identity.Models;

/// <summary>
/// Table de jointure entre les utilisateurs et les rôles.
/// </summary>
public class UserRole
{
    /// <summary>
    /// Identifiant de l'utilisateur.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Identifiant du rôle attribué.
    /// </summary>
    public Guid RoleId { get; set; }
}
