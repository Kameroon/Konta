using Dapper;
using Konta.Finance.Core.Data.Repositories.Interfaces;
using Konta.Finance.Core.Models;
using Konta.Shared.Data;
using Konta.Shared.Data.Repositories;

namespace Konta.Finance.Core.Data.Repositories.Implementations;

/// <summary>
/// Gestion de la persistance des comptes de trésorerie.
/// </summary>
public class TreasuryRepository : BaseRepository<TreasuryRepository>, ITreasuryRepository
{
    public TreasuryRepository(IDbConnectionFactory connectionFactory, ILogger<TreasuryRepository> logger) 
        : base(connectionFactory, logger)
    {
    }

    /// <summary>
    /// Récupère un compte de trésorerie par son identifiant unique.
    /// </summary>
    public async Task<TreasuryAccount?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT * FROM finance_core.TreasuryAccounts WHERE Id = @Id";
        
        _logger.LogDebug("Recherche compte trésorerie : {Id}", id);
        
        using var connection = CreateConnection(sql, new { Id = id });
        return await connection.QuerySingleOrDefaultAsync<TreasuryAccount>(sql, new { Id = id });
    }

    /// <summary>
    /// Liste tous les comptes de trésorerie d'une entreprise.
    /// </summary>
    public async Task<IEnumerable<TreasuryAccount>> GetByTenantIdAsync(Guid tenantId)
    {
        const string sql = "SELECT * FROM finance_core.TreasuryAccounts WHERE TenantId = @TenantId ORDER BY Name ASC";
        
        _logger.LogDebug("Listing des comptes trésorerie pour le tenant : {TenantId}", tenantId);
        
        using var connection = CreateConnection(sql, new { TenantId = tenantId });
        return await connection.QueryAsync<TreasuryAccount>(sql, new { TenantId = tenantId });
    }

    /// <summary>
    /// Met à jour le solde d'un compte.
    /// </summary>
    public async Task<bool> UpdateBalanceAsync(Guid id, decimal newBalance)
    {
        const string sql = "UPDATE finance_core.TreasuryAccounts SET CurrentBalance = @Balance, UpdatedAt = @UpdatedAt WHERE Id = @Id";
        
        _logger.LogInformation("Mise à jour du solde compte {Id} : nouveau solde = {Balance}", id, newBalance);
        
        var parameters = new { Id = id, Balance = newBalance, UpdatedAt = DateTime.UtcNow };
        using var connection = CreateConnection(sql, parameters);
        var rows = await connection.ExecuteAsync(sql, parameters);
        return rows > 0;
    }
}
