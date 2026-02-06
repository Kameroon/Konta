using Ocelot.DependencyInjection;
using Konta.Shared.Extensions;

namespace Konta.Gateway.Extensions;

public static class GatewayServiceExtensions
{
    public static IServiceCollection AddGatewayServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Infrastructure partagée (Db, logging, etc.)
        services.AddSharedInfrastructure(configuration);

        // Blindage : Observabilité & Résilience
        services.AddObservability("Konta.Gateway", configuration);
        services.AddResilience();

        // Authentification JWT
        services.AddAuthenticationConfig(configuration);

        // CORS pour le frontend
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(
                    "http://localhost:5173",
                    "https://localhost:5173",
                    "https://mango-plant-028033703.6.azurestaticapps.net"
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
            });
        });

        // Ocelot
        services.AddOcelot(configuration);

        // Swagger/Explorer
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }

    public static IConfigurationBuilder AddOcelotConfig(this IConfigurationBuilder builder, IWebHostEnvironment environment)
    {
        // Chargement depuis la racine pour plus de simplicité en production
        builder.AddJsonFile("ocelot.json", optional: true, reloadOnChange: true);
        builder.AddJsonFile($"ocelot.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
        
        return builder;
    }
}
