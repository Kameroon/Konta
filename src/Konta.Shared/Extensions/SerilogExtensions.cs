using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Konta.Shared.Extensions;

/// <summary>
/// Extensions pour configurer Serilog de manière centralisée pour tous les microservices.
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Configure Serilog avec Console, File et Seq pour un microservice donné.
    /// </summary>
    /// <param name="builder">Le WebApplicationBuilder du microservice.</param>
    /// <param name="serviceName">Nom du microservice (ex: "Konta.Identity").</param>
    /// <returns>Le WebApplicationBuilder pour chaînage.</returns>
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder, string serviceName)
    {
        // Configuration de Serilog
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration
                // Lecture de la configuration depuis appsettings.json
                .ReadFrom.Configuration(context.Configuration)
                
                // Enrichissement avec des métadonnées
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty("ServiceName", serviceName)
                
                // Sink 1 : Console (pour développement local)
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}"
                )
                
                // Sink 2 : Fichiers (rotation quotidienne, 30 jours de rétention)
                .WriteTo.File(
                    path: $"logs/{serviceName.ToLower()}-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}"
                )
                
                // Sink 3 : Seq (si configuré)
                .WriteTo.Seq(
                    serverUrl: context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341",
                    apiKey: context.Configuration["Seq:ApiKey"] ?? null,
                    restrictedToMinimumLevel: LogEventLevel.Information
                );
        });

        return builder;
    }
}
