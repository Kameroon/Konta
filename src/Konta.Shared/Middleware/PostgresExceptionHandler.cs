using Konta.Shared.Responses;
using Konta.Shared.Services.Postgres;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Konta.Shared.Middleware;

/// <summary>
/// Gestionnaire d'exceptions spécifique pour les erreurs PostgreSQL.
/// Transforme les PostgresException en réponses API structurées avec PostgresErrorResult.
/// </summary>
public class PostgresExceptionHandler : IExceptionHandler
{
    private readonly PostgresErrorService _errorService;
    private readonly ILogger<PostgresExceptionHandler> _logger;

    public PostgresExceptionHandler(
        PostgresErrorService errorService,
        ILogger<PostgresExceptionHandler> logger)
    {
        _errorService = errorService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Gestion des PostgresException directes
        if (exception is PostgresException pgEx)
        {
            _logger.LogWarning(pgEx, "Erreur PostgreSQL interceptée : {SqlState} - {Message}", 
                pgEx.SqlState, pgEx.Message);

            var diagnosis = _errorService.Diagnose(pgEx);
            var errorResult = new PostgresErrorResult(diagnosis);

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(errorResult, cancellationToken);
            return true;
        }

        // Gestion des InvalidOperationException qui encapsulent des erreurs DB
        // (pour compatibilité avec le code existant qui fait throw new InvalidOperationException(diagnosis.Message))
        if (exception is InvalidOperationException && 
            exception.InnerException is PostgresException innerPgEx)
        {
            _logger.LogWarning(innerPgEx, "Erreur PostgreSQL encapsulée interceptée : {SqlState} - {Message}", 
                innerPgEx.SqlState, innerPgEx.Message);

            var diagnosis = _errorService.Diagnose(innerPgEx);
            var errorResult = new PostgresErrorResult(diagnosis);

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(errorResult, cancellationToken);
            return true;
        }

        // Ne gère pas cette exception, laisse passer au prochain handler
        return false;
    }
}
