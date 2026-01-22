using Dapper;
using Konta.Reporting.Data.Repositories.Interfaces;
using Konta.Reporting.Models.Analytics;
using Konta.Shared.Data;
using Konta.Shared.Data.Repositories;

namespace Konta.Reporting.Data.Repositories.Implementations;

/// <summary>
/// Implémentation haute performance du dépôt de reporting utilisant des requêtes SQL d'agrégation.
/// </summary>
public class ReportingRepository : BaseRepository<ReportingRepository>, IReportingRepository
{
    public ReportingRepository(IDbConnectionFactory connectionFactory, ILogger<ReportingRepository> logger) 
        : base(connectionFactory, logger)
    {
    }

    /// <summary>
    /// Résumé financier agrégé (Normalement effectué via cross-DB ou snapshots).
    /// </summary>
    public async Task<FinancialSummary> GetFinancialSummaryAsync(Guid tenantId, string period)
    {
        _logger.LogInformation("Génération du résumé financier pour Tenant {TenantId}, Période {Period}", tenantId, period);

        // Note: Dans un environnement réel, cette requête taperait dans des vues matérialisées
        // ou des tables de synthèse pré-calculées.
        const string sql = @"
            SELECT 
                @TenantId as TenantId,
                @Period as Period,
                COALESCE(SUM((data->>'revenue')::decimal), 0) as TotalRevenue,
                COALESCE(SUM((data->>'expenses')::decimal), 0) as TotalExpenses
            FROM reporting.ReportingSnapshots
            WHERE TenantId = @TenantId AND ReferenceDate >= DATE_TRUNC('month', CURRENT_DATE)
        ";
        
        using var connection = CreateConnection(sql, new { TenantId = tenantId, Period = period });
        var result = await connection.QuerySingleOrDefaultAsync<FinancialSummary>(sql, new { TenantId = tenantId, Period = period });
        
        return result ?? new FinancialSummary { TenantId = tenantId, Period = period };
    }

    /// <summary>
    /// Récupère la tendance des flux de trésorerie.
    /// </summary>
    public async Task<IEnumerable<CashFlowTrend>> GetCashFlowTrendAsync(Guid tenantId, int days)
    {
        _logger.LogDebug("Calcul de la tendance de trésorerie sur {Days} jours pour {TenantId}", days, tenantId);

        // Récupération des données agrégées
        const string sql = @"
            SELECT 
                ReferenceDate as Date,
                (Data->>'balance')::decimal as Balance,
                (Data->>'inflow')::decimal as Inflow,
                (Data->>'outflow')::decimal as Outflow
            FROM reporting.ReportingSnapshots
            WHERE TenantId = @TenantId AND SnapshotType = 'DailyCash'
            ORDER BY ReferenceDate DESC
            LIMIT @Limit";

        using var connection = CreateConnection(sql, new { TenantId = tenantId, Limit = days });
        return await connection.QueryAsync<CashFlowTrend>(sql, new { TenantId = tenantId, Limit = days });
    }

    /// <summary>
    /// Répartition du CA par catégorie.
    /// </summary>
    public async Task<IEnumerable<DashboardKpi>> GetRevenueBreakdownAsync(Guid tenantId)
    {
        _logger.LogDebug("Récupération du breakdown des revenus pour {TenantId}", tenantId);

        const string sql = @"
            SELECT 
                key as Name, 
                value::decimal as Value,
                '€' as Unit
            FROM reporting.ReportingSnapshots, jsonb_each_text(Data->'breakdown')
            WHERE TenantId = @TenantId AND SnapshotType = 'RevenueBreakdown'
            LIMIT 5";

        using var connection = CreateConnection(sql, new { TenantId = tenantId });
        return await connection.QueryAsync<DashboardKpi>(sql, new { TenantId = tenantId });
    }
}
