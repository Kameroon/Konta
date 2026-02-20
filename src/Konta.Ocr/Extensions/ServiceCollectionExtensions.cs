using Konta.Ocr.Data.Repositories.Implementations;
using Konta.Ocr.Data.Repositories.Interfaces;
using Konta.Ocr.Services.Implementations;
using Konta.Ocr.Services.Interfaces;
using Konta.Ocr.BackgroundServices;
using Konta.Shared.Extensions;
using Konta.Shared.Middleware;

namespace Konta.Ocr.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOcrInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedInfrastructure(configuration);
        services.AddAuthenticationConfig(configuration);

        // Configuration Azure AI
        services.Configure<AzureAiOptions>(configuration.GetSection("AzureAI"));

        // Repositories
        services.AddScoped<IExtractionJobRepository, ExtractionJobRepository>();

        // Initialisation de la base de données
        services.AddScoped<Konta.Ocr.Data.DatabaseInitializer>();

        return services;
    }

    public static IServiceCollection AddOcrServices(this IServiceCollection services)
    {
        // Extraction Services
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IAzureAiExtractionService, AzureAiExtractionService>();
        services.AddScoped<IExtractionService, ExtractionService>();

        // Background Worker
        services.AddHostedService<ExtractionBackgroundWorker>();

        // HTTP Client pour l'IA (enregistre aussi IAiParsingService)
        services.AddHttpClient<IAiParsingService, AiParsingService>();

        // Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Exception Handling
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
