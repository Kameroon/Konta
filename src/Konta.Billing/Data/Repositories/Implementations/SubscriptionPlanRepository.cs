using Dapper;
using Konta.Billing.Data.Repositories.Interfaces;
using Konta.Billing.Models;
using Konta.Shared.Data;
using Konta.Shared.Data.Repositories;

namespace Konta.Billing.Data.Repositories.Implementations;

/// <summary>
/// Implémentation Dapper du dépôt des plans d'abonnement.
/// </summary>
public class SubscriptionPlanRepository : BaseRepository<SubscriptionPlanRepository>, ISubscriptionPlanRepository
{
    public SubscriptionPlanRepository(IDbConnectionFactory connectionFactory, ILogger<SubscriptionPlanRepository> logger) 
        : base(connectionFactory, logger)
    {
    }

    /// <summary>
    /// Récupère tous les plans d'abonnement actifs ordonnés par prix croissant.
    /// </summary>
    public async Task<IEnumerable<SubscriptionPlan>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM billing.SubscriptionPlans WHERE IsActive = TRUE ORDER BY Price ASC";
        using var connection = CreateConnection(sql);
        return await connection.QueryAsync<SubscriptionPlan>(sql);
    }

    /// <summary>
    /// Récupère un plan d'abonnement spécifique via son code unique.
    /// </summary>
    /// <param name="code">Le code technique (ex: 'premium').</param>
    public async Task<SubscriptionPlan?> GetByCodeAsync(string code)
    {
        const string sql = "SELECT * FROM billing.SubscriptionPlans WHERE Code = @Code AND IsActive = TRUE";
        using var connection = CreateConnection(sql, new { Code = code });
        return await connection.QueryFirstOrDefaultAsync<SubscriptionPlan>(sql, new { Code = code });
    }
}
