using Microsoft.Extensions.Caching.Memory;
using Konta.Reporting.Data.Repositories.Interfaces;
using Konta.Reporting.Models.Analytics;
using Konta.Reporting.Services.Interfaces;

namespace Konta.Reporting.Services.Implementations;

/// <summary>
/// Service gérant les KPIs avec une stratégie de cache agressive pour la performance.
/// </summary>
public class KpiService : IKpiService
{
    private readonly IReportingRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<KpiService> _logger;
    private const string CacheKeyPrefix = "Konta_KPI_";

    public KpiService(
        IReportingRepository repository, 
        IMemoryCache cache, 
        ILogger<KpiService> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Récupère le résumé financier en vérifiant d'abord le cache mémoire.
    /// </summary>
    public async Task<FinancialSummary> GetDashboardSummaryAsync(Guid tenantId)
    {
        var cacheKey = $"{CacheKeyPrefix}Summary_{tenantId}";
        
        // Tentative de récupération depuis le cache
        if (_cache.TryGetValue(cacheKey, out FinancialSummary? summary) && summary != null)
        {
            _logger.LogDebug("🚀 Récupération du résumé financier depuis le CACHE pour {TenantId}", tenantId);
            return summary;
        }

        _logger.LogInformation("📥 Cache miss pour le résumé de {TenantId}. Calcul en cours...", tenantId);
        
        // Calcul et mise en cache pour 15 minutes par défaut
        var period = DateTime.UtcNow.ToString("yyyy-MM");
        summary = await _repository.GetFinancialSummaryAsync(tenantId, period);

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(15))
            .SetAbsoluteExpiration(TimeSpan.FromHours(1));

        _cache.Set(cacheKey, summary, cacheOptions);

        return summary;
    }

    /// <summary>
    /// Calcule les KPIs financiers à la volée.
    /// </summary>
    public async Task<IEnumerable<DashboardKpi>> GetMainKpisAsync(Guid tenantId)
    {
        _logger.LogInformation("Calcul des KPIs principaux pour {TenantId}", tenantId);
        
        var summary = await GetDashboardSummaryAsync(tenantId);
        
        // Construction des indicateurs visuels alignés avec le frontend
        return new List<DashboardKpi>
        {
            new DashboardKpi { 
                Label = "Chiffre d'Affaires", 
                Value = summary.TotalRevenue, 
                PreviousValue = summary.TotalRevenue * 0.92m, // Mock : +8%
                Trend = "up",
                Format = "currency",
                Color = "#10b981" // Green-500
            },
            new DashboardKpi { 
                Label = "Marge Brute", 
                Value = summary.GrossMargin, 
                PreviousValue = summary.GrossMargin * 1.05m, // Mock : -5%
                Trend = "down",
                Format = "currency",
                Color = "#3b82f6" // Blue-500
            },
            new DashboardKpi { 
                Label = "Rentabilité", 
                Value = summary.ProfitabilityPercentage, 
                PreviousValue = 22.5m, 
                Trend = summary.ProfitabilityPercentage > 22.5m ? "up" : "down",
                Format = "percentage",
                Color = "#8b5cf6" // Violet-500
            }
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CashFlowTrend>> GetCashFlowTrendAsync(Guid tenantId, int days = 30)
    {
        _logger.LogInformation("Récupération de la tendance de trésorerie pour {TenantId} sur {Days} jours", tenantId, days);
        return await _repository.GetCashFlowTrendAsync(tenantId, days);
    }
}
