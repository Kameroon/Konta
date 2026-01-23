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
    private readonly IPermissionRepository _permissionRepository;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        IRoleRepository roleRepository, 
        IPermissionRepository permissionRepository,
        ILogger<RoleService> logger)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
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

        // ✅ SÉCURITÉ MULTI-TENANT : Vérifier que le rôle existe et récupérer son TenantId
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
        {
            _logger.LogWarning("Tentative d'assignation de permission à un rôle inexistant : {RoleId}", roleId);
            throw new InvalidOperationException($"Le rôle avec l'ID {roleId} n'existe pas.");
        }

        // Note : Dans un contexte authentifié, on devrait vérifier que role.TenantId correspond au tenant de l'utilisateur courant
        // Note : switch (role.Name) { ... }

        // Pour l'instant, on fait confiance au fait que l'endpoint vérifie déjà l'appartenance

        // ✅ VALIDATION : Vérifier que la permission existe
        var permission = await _permissionRepository.GetByIdAsync(request.PermissionId);
        if (permission == null)
        {
            _logger.LogWarning("Tentative d'assignation d'une permission inexistante : {PermissionId}", request.PermissionId);
            throw new InvalidOperationException($"La permission avec l'ID {request.PermissionId} n'existe pas.");
        }

        // ✅ ASSIGNATION : Créer la relation RolePermission
        await _roleRepository.AddPermissionToRoleAsync(roleId, request.PermissionId);
        
        _logger.LogInformation("Permission '{PermissionName}' assignée avec succès au rôle '{RoleName}' (Tenant: {TenantId})", 
            permission.SystemName, role.Name, role.TenantId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RoleResponse>> GetRolesAsync(Guid tenantId)
    {
        _logger.LogInformation("Récupération des rôles pour le tenant {TenantId}", tenantId);
        var roles = await _roleRepository.GetAllByTenantIdAsync(tenantId);
        return roles.Select(r => new RoleResponse
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            IsDefault = r.IsDefault
        });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PermissionResponse>> GetPermissionsByRoleIdAsync(Guid roleId)
    {
        _logger.LogInformation("Récupération des permissions pour le rôle {RoleId}", roleId);
        var permissions = await _roleRepository.GetPermissionsByRoleIdAsync(roleId);
        return permissions.Select(p => new PermissionResponse
        {
            Id = p.Id,
            SystemName = p.SystemName,
            Name = p.Name,
            Description = p.Description
        });
    }
}
