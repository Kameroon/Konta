using Dapper; // Micro-ORM pour l'accès aux données
using Konta.Identity.Data.Repositories.Interfaces; // Definition des contrats
using Konta.Identity.Models; // Modèles du domaine
using Microsoft.Extensions.Logging; // Journalisation
using Konta.Shared.Data; // Interface factory partagée
using Konta.Shared.Data.Repositories; // Classe de base des dépôts

namespace Konta.Identity.Data.Repositories.Implementations;

/// <summary>
/// Dépôt gérant la persistance des permissions.
/// </summary>
public class PermissionRepository : BaseRepository<PermissionRepository>, IPermissionRepository
{
    /// <summary>
    /// Initialise le dépôt des permissions.
    /// </summary>
    public PermissionRepository(IDbConnectionFactory connectionFactory, ILogger<PermissionRepository> logger)
        : base(connectionFactory, logger)
    {
    }

    /// <inheritdoc />
    public async Task<Permission?> GetByIdAsync(Guid permissionId)
    {
        _logger.LogDebug("Accès DB : Recherche de permission par ID : {PermissionId}", permissionId);
        const string sql = "SELECT * FROM identity.Permissions WHERE Id = @PermissionId";
        using var connection = CreateConnection(sql, new { PermissionId = permissionId });
        return await connection.QuerySingleOrDefaultAsync<Permission>(sql, new { PermissionId = permissionId });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetPermissionsByUserIdAsync(Guid userId)
    {
        _logger.LogDebug("Accès DB : Récupération des permissions pour l'utilisateur ID : {UserId}", userId);
        const string sql = @"
            SELECT DISTINCT p.SystemName 
            FROM identity.Permissions p
            JOIN identity.RolePermissions rp ON p.Id = rp.PermissionId
            JOIN identity.UserRoles ur ON rp.RoleId = ur.RoleId
            WHERE ur.UserId = @UserId";
        
        using var connection = CreateConnection(sql, new { UserId = userId });
        return await connection.QueryAsync<string>(sql, new { UserId = userId });
    }

    /// <inheritdoc />
    public async Task<Permission?> GetBySystemNameAsync(string systemName)
    {
        _logger.LogDebug("Accès DB : Recherche de permission par nom système : {SystemName}", systemName);
        const string sql = "SELECT * FROM identity.Permissions WHERE SystemName = @SystemName";
        using var connection = CreateConnection(sql, new { SystemName = systemName });
        return await connection.QuerySingleOrDefaultAsync<Permission>(sql, new { SystemName = systemName });
    }
}
