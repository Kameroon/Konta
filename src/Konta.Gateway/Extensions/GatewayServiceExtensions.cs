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

        // Ocelot
        services.AddOcelot(configuration);

        // Swagger/Explorer
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }

    public static IConfigurationBuilder AddOcelotConfig(this IConfigurationBuilder builder, IWebHostEnvironment environment)
    {
        string folder = "Configuration/Ocelot";
        builder.AddOcelot(folder, environment);
        return builder;
    }
}
