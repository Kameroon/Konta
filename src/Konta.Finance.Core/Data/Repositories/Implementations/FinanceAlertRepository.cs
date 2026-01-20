using Dapper;
using Konta.Finance.Core.Data.Repositories.Interfaces;
using Konta.Finance.Core.Models;
using Konta.Shared.Data;
using Konta.Shared.Data.Repositories;

namespace Konta.Finance.Core.Data.Repositories.Implementations;

/// <summary>
/// Gestion de la persistance des alertes financières.
/// </summary>
public class FinanceAlertRepository : BaseRepository<FinanceAlertRepository>, IFinanceAlertRepository
{
    public FinanceAlertRepository(IDbConnectionFactory connectionFactory, ILogger<FinanceAlertRepository> logger) 
        : base(connectionFactory, logger)
    {
    }

    /// <summary>
    /// Récupère la liste des alertes non lues pour un tenant.
    /// </summary>
    public async Task<IEnumerable<FinanceAlert>> GetUnreadByTenantIdAsync(Guid tenantId)
    {
        const string sql = "SELECT * FROM FinanceAlerts WHERE TenantId = @TenantId AND IsRead = FALSE ORDER BY CreatedAt DESC";
        
        // Log de debug pour tracer la requête
        _logger.LogDebug("Récupération des alertes non lues pour : {TenantId}", tenantId);
        
        using var connection = CreateConnection(sql, new { TenantId = tenantId });
        return await connection.QueryAsync<FinanceAlert>(sql, new { TenantId = tenantId });
    }

    /// <summary>
    /// Enregistre une nouvelle alerte en base.
    /// </summary>
    public async Task<Guid> CreateAsync(FinanceAlert alert)
    {
        const string sql = @"
            INSERT INTO FinanceAlerts (Id, TenantId, Title, Message, Severity, IsRead, RelatedEntityId, CreatedAt)
            VALUES (@Id, @TenantId, @Title, @Message, @Severity, @IsRead, @RelatedEntityId, @CreatedAt)
            RETURNING Id";
            
        _logger.LogInformation("Création d'une alerte : {Title} (Gravité: {Severity})", alert.Title, alert.Severity);
        
        using var connection = CreateConnection(sql, alert);
        return await connection.ExecuteScalarAsync<Guid>(sql, alert);
    }

    /// <summary>
    /// Marque une alerte comme lue.
    /// </summary>
    public async Task<bool> MarkAsReadAsync(Guid id)
    {
        const string sql = "UPDATE FinanceAlerts SET IsRead = TRUE, UpdatedAt = @UpdatedAt WHERE Id = @Id";
        
        _logger.LogDebug("Marquage de l'alerte comme lue : {Id}", id);
        
        var parameters = new { Id = id, UpdatedAt = DateTime.UtcNow };
        using var connection = CreateConnection(sql, parameters);
        var rows = await connection.ExecuteAsync(sql, parameters);
        return rows > 0;
    }
}
