using Konta.Identity.DTOs;

namespace Konta.Identity.Services.Interfaces;

/// <summary>
/// Service de gestion des rôles et des permissions.
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Crée un nouveau rôle pour un tenant donné.
    /// </summary>
    Task<Guid> CreateRoleAsync(Guid tenantId, CreateRoleRequest request);

    /// <summary>
    /// Assigne une permission à un rôle.
    /// </summary>
    Task AssignPermissionAsync(Guid roleId, AssignPermissionRequest request);
}
