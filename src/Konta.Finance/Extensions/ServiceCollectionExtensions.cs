using Konta.Finance.Services.Implementations;
using Konta.Finance.Services.Interfaces;
using Konta.Finance.Data.Repositories.Implementations;
using Konta.Finance.Data.Repositories.Interfaces;
using Konta.Shared.Extensions;
using Konta.Shared.Middleware;

namespace Konta.Finance.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFinanceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Shared Infrastructure & Authentication
        services.AddSharedInfrastructure(configuration);
        services.AddAuthenticationConfig(configuration);

        // Repositories
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IJournalRepository, JournalRepository>();
        services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();

        // Initialisation de la base de données
        services.AddScoped<Konta.Finance.Data.DatabaseInitializer>();

        return services;
    }

    public static IServiceCollection AddFinanceServices(this IServiceCollection services)
    {
        // Domain Services
        services.AddScoped<IAccountingService, AccountingService>();

        // Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Exception Handling
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
