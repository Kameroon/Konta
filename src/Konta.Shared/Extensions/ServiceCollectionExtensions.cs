using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using Konta.Shared.Resilience;
using Konta.Shared.Middleware;
using Konta.Shared.Services.Postgres;
using Konta.Shared.Data;
using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using MediatR;

namespace Konta.Shared.Extensions;

/// <summary>
/// Méthodes d'extension pour IServiceCollection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Ajoute les services partagés à l'injection de dépendances.
    /// </summary>
    /// <param name="services">La collection de services.</param>
    /// <param name="configurePostgresErrors">Optionnel : Action pour configurer les erreurs Postgres.</param>
    /// <returns>La collection de services pour le chaînage.</returns>
    public static IServiceCollection AddSharedServices(
        this IServiceCollection services, 
        Action<PostgresErrorOptions>? configurePostgresErrors = null)
    {
        // Configuration des options
        if (configurePostgresErrors != null)
        {
            services.Configure(configurePostgresErrors);
        }
        else
        {
            // Enregistre les options par défaut si aucune configuration n'est fournie
            services.Configure<PostgresErrorOptions>(_ => { });
        }

        services.AddSingleton<PostgresErrorService>();
        
        // Enregistrement du handler d'exceptions PostgreSQL
        services.AddExceptionHandler<PostgresExceptionHandler>();
        
        // Enregistrement de MediatR pour le découplage inter-modules (Modular Monolith)
        services.AddMediatR(cfg => {
            // On scanne les assemblies chargés pour trouver les handlers
            cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
        });

        // Configuration JSON (Enums en chaînes de caractères par défaut)
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
        });

        return services;
    }

    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Tenant Context (Scoped pour durer le temps d'une requête)
        services.AddScoped<ITenantContext, TenantContext>();

        // Database
        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
        return services;
    }

    public static IServiceCollection AddAuthenticationConfig(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"];
        
        if (string.IsNullOrEmpty(secret)) return services;

        var key = Encoding.UTF8.GetBytes(secret);

        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = false;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Configure l'observabilité (Tracing) via OpenTelemetry.
    /// </summary>
    public static IServiceCollection AddObservability(this IServiceCollection services, string serviceName, IConfiguration configuration)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter(); // En production, on utiliserait AddOtlpExporter
            });

        return services;
    }

    /// <summary>
    /// Configure la résilience globale via Polly.
    /// </summary>
    public static IServiceCollection AddResilience(this IServiceCollection services)
    {
        services.AddResiliencePipeline(ResilienceConstants.DefaultPolicy, builder =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = ResilienceConstants.Retry.Count,
                Delay = ResilienceConstants.Retry.Delay,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = ResilienceConstants.CircuitBreaker.FailureThreshold,
                SamplingDuration = ResilienceConstants.CircuitBreaker.SamplingDuration,
                BreakDuration = ResilienceConstants.CircuitBreaker.BreakDuration
            });
        });

        return services;
    }

    public static IApplicationBuilder UseTenantContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantMiddleware>();
    }
}
