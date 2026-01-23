using Dapper; // Micro-ORM pour l'accès aux données
using Konta.Identity.Data.Repositories.Interfaces; // Definition des contrats
using Konta.Identity.Models; // Modèles du domaine
using System.Data; // Objets SQL ADO.NET
using Microsoft.Extensions.Logging; // Journalisation
using Konta.Shared.Data; // Interface factory partagée
using Konta.Shared.Data.Repositories; // Classe de base des dépôts

namespace Konta.Identity.Data.Repositories.Implementations;

/// <summary>
/// Dépôt gérant la persistance des rôles.
/// </summary>
public class RoleRepository : BaseRepository<RoleRepository>, IRoleRepository
{
    /// <summary>
    /// Initialise le dépôt des rôles.
    /// </summary>
    public RoleRepository(IDbConnectionFactory connectionFactory, ILogger<RoleRepository> logger)
        : base(connectionFactory, logger)
    {
    }

    /// <inheritdoc />
    public async Task<Guid> CreateAsync(Role role)
    {
        _logger.LogInformation("Accès DB : Création du rôle : {RoleName}", role.Name);
        const string sql = @"
            INSERT INTO identity.Roles (Id, TenantId, Name, Description, IsDefault, CreatedAt) 
            VALUES (@Id, @TenantId, @Name, @Description, @IsDefault, @CreatedAt)
            RETURNING Id";
        using var connection = CreateConnection(sql, role);
        return await connection.ExecuteScalarAsync<Guid>(sql, role);
    }

    /// <inheritdoc />
    public async Task<Role?> GetByIdAsync(Guid roleId)
    {
        _logger.LogDebug("Accès DB : Recherche de rôle par ID : {RoleId}", roleId);
        const string sql = "SELECT * FROM identity.Roles WHERE Id = @RoleId";
        using var connection = CreateConnection(sql, new { RoleId = roleId });
        return await connection.QuerySingleOrDefaultAsync<Role>(sql, new { RoleId = roleId });
    }

    /// <inheritdoc />
    public async Task<Role?> GetByNameAsync(Guid tenantId, string roleName)
    {
        _logger.LogDebug("Accès DB : Recherche de rôle par nom : {RoleName}", roleName);
        const string sql = "SELECT * FROM identity.Roles WHERE TenantId = @TenantId AND Name = @Name";
        using var connection = CreateConnection(sql, new { TenantId = tenantId, Name = roleName });
        return await connection.QuerySingleOrDefaultAsync<Role>(sql, new { TenantId = tenantId, Name = roleName });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Role>> GetByUserIdAsync(Guid userId)
    {
        _logger.LogDebug("Accès DB : Récupération des rôles pour l'utilisateur : {UserId}", userId);
        const string sql = @"
            SELECT r.* 
            FROM identity.Roles r
            JOIN identity.UserRoles ur ON r.Id = ur.RoleId
            WHERE ur.UserId = @UserId";
        using var connection = CreateConnection(sql, new { UserId = userId });
        return await connection.QueryAsync<Role>(sql, new { UserId = userId });
    }

    /// <inheritdoc />
    public async Task AddPermissionToRoleAsync(Guid roleId, Guid permissionId)
    {
        _logger.LogDebug("Accès DB : Ajout permission {PermissionId} au rôle {RoleId}", permissionId, roleId);
        const string sql = "INSERT INTO identity.RolePermissions (RoleId, PermissionId) VALUES (@RoleId, @PermissionId)";
        using var connection = CreateConnection(sql, new { RoleId = roleId, PermissionId = permissionId });
        await connection.ExecuteAsync(sql, new { RoleId = roleId, PermissionId = permissionId });
    }

    /// <inheritdoc />
    public async Task AssignRoleToUserAsync(Guid userId, Guid roleId)
    {
        _logger.LogDebug("Accès DB : Assignation rôle {RoleId} à l'utilisateur {UserId}", roleId, userId);
        const string sql = "INSERT INTO identity.UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)";
        using var connection = CreateConnection(sql, new { UserId = userId, RoleId = roleId });
        await connection.ExecuteAsync(sql, new { UserId = userId, RoleId = roleId });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Role>> GetAllByTenantIdAsync(Guid tenantId)
    {
        _logger.LogDebug("Accès DB : Récupération des rôles pour le tenant : {TenantId}", tenantId);
        const string sql = "SELECT * FROM identity.Roles WHERE TenantId = @TenantId";
        using var connection = CreateConnection(sql, new { TenantId = tenantId });
        return await connection.QueryAsync<Role>(sql, new { TenantId = tenantId });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId)
    {
        _logger.LogDebug("Accès DB : Récupération des permissions pour le rôle : {RoleId}", roleId);
        const string sql = @"
            SELECT p.* 
            FROM identity.Permissions p
            JOIN identity.RolePermissions rp ON p.Id = rp.PermissionId
            WHERE rp.RoleId = @RoleId";
        using var connection = CreateConnection(sql, new { RoleId = roleId });
        return await connection.QueryAsync<Permission>(sql, new { RoleId = roleId });
    }
}
