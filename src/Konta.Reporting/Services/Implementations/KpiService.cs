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
        
        // Construction des indicateurs visuels
        return new List<DashboardKpi>
        {
            new DashboardKpi { 
                Name = "Chiffre d'Affaires", 
                Value = summary.TotalRevenue, 
                TrendPercentage = 5.2m, 
                Status = "Ok" 
            },
            new DashboardKpi { 
                Name = "Marge Brute", 
                Value = summary.GrossMargin, 
                TrendPercentage = -1.5m, 
                Status = summary.GrossMargin > 0 ? "Ok" : "Critical" 
            },
            new DashboardKpi { 
                Name = "Rentabilité", 
                Value = summary.ProfitabilityPercentage, 
                Unit = "%", 
                Status = summary.ProfitabilityPercentage > 15 ? "Ok" : "Warning" 
            }
        };
    }
}
