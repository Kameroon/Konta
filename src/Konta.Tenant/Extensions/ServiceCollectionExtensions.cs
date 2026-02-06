using FluentValidation;
using Konta.Tenant.Data.Repositories.Implementations;
using Konta.Tenant.Data.Repositories.Interfaces;
using Konta.Tenant.Services.Implementations;
using Konta.Tenant.Services.Interfaces;
using Konta.Shared.Data;
using Konta.Shared.Extensions;
using Konta.Shared.Middleware;

namespace Konta.Tenant.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Shared Core & Database
        services.AddSharedInfrastructure(configuration);
        
        // Repositories
        services.AddScoped<ITenantRepository, TenantRepository>();

        // Initialisation de la base de données
        services.AddScoped<Konta.Tenant.Data.DatabaseInitializer>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Domain Services
        services.AddScoped<ITenantService, TenantService>();

        // Validators
        services.AddValidatorsFromAssemblyContaining<Program>();

        // Swagger & OpenApi
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Exception Handling
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
