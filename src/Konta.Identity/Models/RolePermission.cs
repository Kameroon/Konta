namespace Konta.Identity.Models;

/// <summary>
/// Table de jointure entre les rôles et les permissions.
/// </summary>
public class RolePermission
{
    /// <summary>
    /// Identifiant du rôle.
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Identifiant de la permission.
    /// </summary>
    public Guid PermissionId { get; set; }
}
