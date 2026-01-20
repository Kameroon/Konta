using Konta.Identity.Data.Repositories.Interfaces;
using Konta.Identity.DTOs;
using Konta.Identity.Models;
using Konta.Identity.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Konta.Identity.Services.Implementations;

/// <summary>
/// Implémentation du service de gestion des rôles.
/// </summary>
public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<RoleService> _logger;

    public RoleService(IRoleRepository roleRepository, ILogger<RoleService> logger)
    {
        _roleRepository = roleRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Guid> CreateRoleAsync(Guid tenantId, CreateRoleRequest request)
    {
        _logger.LogInformation("Création du rôle '{RoleName}' pour le tenant {TenantId}", request.Name, tenantId);

        // Ici, on pourrait ajouter une validation métier (ex: unicité du nom du rôle pour ce tenant)
        var role = new Role
        {
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description
        };

        var roleId = await _roleRepository.CreateAsync(role);
        
        _logger.LogInformation("Rôle créé avec succès. ID : {RoleId}", roleId);
        return roleId;
    }

    /// <inheritdoc />
    public async Task AssignPermissionAsync(Guid roleId, AssignPermissionRequest request)
    {
        _logger.LogInformation("Assignation de la permission {PermissionId} au rôle {RoleId}", request.PermissionId, roleId);

        // TODO: Vérifier si le rôle appartient bien au tenant de l'utilisateur courant (Sécurité Multi-tenant)
        // TODO: Vérifier si la permission existe

        await _roleRepository.AddPermissionToRoleAsync(roleId, request.PermissionId);
        
        _logger.LogInformation("Permission assignée avec succès.");
    }
}
