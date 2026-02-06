using Konta.Finance.Core.Data.Repositories.Implementations;
using Konta.Finance.Core.Data.Repositories.Interfaces;
using Konta.Finance.Core.Services.Implementations;
using Konta.Finance.Core.Services.Interfaces;
using Konta.Shared.Extensions;
using Konta.Shared.Middleware;

namespace Konta.Finance.Core.Extensions;

/// <summary>
/// Extensions pour la configuration des services de Finance Core.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configure l'infrastructure de base (DB, Auth).
    /// </summary>
    public static IServiceCollection AddFinanceCoreInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Enregistre l'infrastructure partagée (DbConnectionFactory)
        services.AddSharedInfrastructure(configuration);
        
        // Configure l'authentification JWT avec les secrets synchronisés
        services.AddAuthenticationConfig(configuration);

        // Enregistrement des dépôts de données (Scoped pour la durée de la requête)
        services.AddScoped<ITierRepository, TierRepository>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<ITreasuryRepository, TreasuryRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IFinanceAlertRepository, FinanceAlertRepository>();

        // Initialisation de la base de données
        services.AddScoped<Konta.Finance.Core.Data.DatabaseInitializer>();

        return services;
    }

    /// <summary>
    /// Configure les services métier et les outils API.
    /// </summary>
    public static IServiceCollection AddFinanceCoreServices(this IServiceCollection services)
    {
        // Enregistrement des services domaine
        services.AddScoped<IBudgetService, BudgetService>();
        services.AddScoped<ITreasuryService, TreasuryService>();

        // Configuration de la documentation Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Gestion globale des exceptions (Middleware partagé)
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
