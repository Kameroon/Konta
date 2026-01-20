using Dapper;
using Konta.Finance.Core.Data.Repositories.Interfaces;
using Konta.Finance.Core.Models;
using Konta.Shared.Data;
using Konta.Shared.Data.Repositories;

namespace Konta.Finance.Core.Data.Repositories.Implementations;

/// <summary>
/// Gestion de la persistance des tiers (Clients/Fournisseurs).
/// </summary>
public class TierRepository : BaseRepository<TierRepository>, ITierRepository
{
    public TierRepository(IDbConnectionFactory connectionFactory, ILogger<TierRepository> logger) 
        : base(connectionFactory, logger)
    {
    }

    /// <summary>
    /// Récupère un tiers par son ID.
    /// </summary>
    public async Task<Tier?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT * FROM finance_core.Tiers WHERE Id = @Id";
        
        _logger.LogDebug("Recherche du tiers : {Id}", id);
        
        using var connection = CreateConnection(sql, new { Id = id });
        return await connection.QuerySingleOrDefaultAsync<Tier>(sql, new { Id = id });
    }

    /// <summary>
    /// Liste les tiers d'un tenant, filtrable par type (Client/Fournisseur).
    /// </summary>
    public async Task<IEnumerable<Tier>> GetByTenantIdAsync(Guid tenantId, TierType? type = null)
    {
        var sql = "SELECT * FROM finance_core.Tiers WHERE TenantId = @TenantId";
        if (type.HasValue) sql += " AND Type = @Type";
        sql += " ORDER BY Name ASC";

        _logger.LogDebug("Listing des tiers pour Tenant : {TenantId} (Type: {Type})", tenantId, type);

        using var connection = CreateConnection(sql, new { TenantId = tenantId, Type = type });
        return await connection.QueryAsync<Tier>(sql, new { TenantId = tenantId, Type = type });
    }

    /// <summary>
    /// Crée un nouveau tiers en base de données.
    /// </summary>
    public async Task<Guid> CreateAsync(Tier tier)
    {
        const string sql = @"
            INSERT INTO finance_core.Tiers (Id, TenantId, Name, Type, Email, TaxId, Address, CreatedAt)
            VALUES (@Id, @TenantId, @Name, @Type, @Email, @TaxId, @Address, @CreatedAt)
            RETURNING Id";
            
        _logger.LogInformation("Création d'un nouveau tiers : {Name} (Type: {Type})", tier.Name, tier.Type);
        
        using var connection = CreateConnection(sql, tier);
        return await connection.ExecuteScalarAsync<Guid>(sql, tier);
    }

    /// <summary>
    /// Met à jour les informations d'un tiers.
    /// </summary>
    public async Task<bool> UpdateAsync(Tier tier)
    {
        const string sql = @"
            UPDATE finance_core.Tiers 
            SET Name = @Name, Type = @Type, Email = @Email, TaxId = @TaxId, Address = @Address, UpdatedAt = @UpdatedAt
            WHERE Id = @Id";
            
        _logger.LogInformation("Mise à jour du tiers : {Id}", tier.Id);
        
        tier.UpdatedAt = DateTime.UtcNow;
        using var connection = CreateConnection(sql, tier);
        var rows = await connection.ExecuteAsync(sql, tier);
        return rows > 0;
    }
}
