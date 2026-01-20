using Dapper; // Micro-ORM pour l'accès aux données
using Konta.Identity.Data.Repositories.Interfaces; // Definition des contrats
using Konta.Identity.Models; // Modèles du domaine
using Microsoft.Extensions.Logging; // Journalisation
using System.Data; // Objets SQL ADO.NET
using Konta.Shared.Data; // Interface factory partagée
using Konta.Shared.Data.Repositories; // Classe de base des dépôts

namespace Konta.Identity.Data.Repositories.Implementations;

/// <summary>
/// Dépôt gérant la persistance des entreprises (Tenants) au niveau de l'identité.
/// </summary>
public class TenantRepository : BaseRepository<TenantRepository>, ITenantRepository
{
    /// <summary>
    /// Constructeur avec injection des dépendances.
    /// </summary>
    public TenantRepository(IDbConnectionFactory connectionFactory, ILogger<TenantRepository> logger)
        : base(connectionFactory, logger)
    {
    }

    /// <inheritdoc />
    public async Task<Tenant?> GetByIdAsync(Guid id)
    {
        _logger.LogDebug("Accès DB : Récupération du tenant par ID : {Id}", id);
        const string sql = "SELECT * FROM Tenants WHERE Id = @Id";
        using var connection = CreateConnection(sql, new { Id = id });
        return await connection.QuerySingleOrDefaultAsync<Tenant>(sql, new { Id = id });
    }

    /// <inheritdoc />
    public async Task<Tenant?> GetByNameAsync(string name)
    {
        _logger.LogDebug("Accès DB : Récupération du tenant par nom : {Name}", name);
        const string sql = "SELECT * FROM Tenants WHERE Name = @Name";
        using var connection = CreateConnection(sql, new { Name = name });
        return await connection.QuerySingleOrDefaultAsync<Tenant>(sql, new { Name = name });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Tenant>> GetAllAsync()
    {
        _logger.LogDebug("Accès DB : Récupération de tous les tenants");
        const string sql = "SELECT * FROM Tenants ORDER BY CreatedAt DESC";
        using var connection = CreateConnection(sql);
        return await connection.QueryAsync<Tenant>(sql);
    }

    /// <inheritdoc />
    public async Task<Guid> CreateAsync(Tenant tenant)
    {
        _logger.LogInformation("Accès DB : Création du tenant : {Name}", tenant.Name);
        const string sql = @"
            INSERT INTO Tenants (Id, Name, Plan, CreatedAt) 
            VALUES (@Id, @Name, @Plan, @CreatedAt)
            RETURNING Id";
        
        using var connection = CreateConnection(sql, tenant);
        return await connection.ExecuteScalarAsync<Guid>(sql, tenant);
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(Tenant tenant)
    {
        _logger.LogInformation("Accès DB : Mise à jour du tenant ID : {Id}", tenant.Id);
        const string sql = @"
            UPDATE Tenants 
            SET Name = @Name, 
                Plan = @Plan, 
                UpdatedAt = @UpdatedAt 
            WHERE Id = @Id";
        
        using var connection = CreateConnection(sql, tenant);
        var affectedRows = await connection.ExecuteAsync(sql, tenant);
        return affectedRows > 0;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogWarning("Accès DB : Suppression du tenant ID : {Id}", id);
        const string sql = "DELETE FROM Tenants WHERE Id = @Id";
        using var connection = CreateConnection(sql, new { Id = id });
        var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
        return affectedRows > 0;
    }
}
