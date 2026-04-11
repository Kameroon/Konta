using Konta.Reporting.Data.Repositories.Implementations;
using Konta.Reporting.Data.Repositories.Interfaces;
using Konta.Reporting.Services.Implementations;
using Konta.Reporting.Services.Interfaces;
using Konta.Shared.Extensions;
using Konta.Shared.Middleware;

namespace Konta.Reporting.Extensions;

/// <summary>
/// Configuration de l'injection de dépendances pour le reporting.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReportingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Mise en cache
        services.AddMemoryCache();

        // Repositories
        services.AddScoped<IReportingRepository, ReportingRepository>();

        // Initialisation de la base de données
        services.AddScoped<Konta.Reporting.Data.DatabaseInitializer>();

        return services;
    }

    public static IServiceCollection AddReportingServices(this IServiceCollection services)
    {
        // Services métier analytiques
        services.AddScoped<IKpiService, KpiService>();
        services.AddScoped<IExportService, ExportService>();

        return services;
    }
}
