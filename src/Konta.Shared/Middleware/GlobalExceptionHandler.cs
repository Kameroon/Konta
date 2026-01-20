using Konta.Shared.Responses;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Konta.Shared.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Une erreur non gérée est survenue : {Message}", exception.Message);

        httpContext.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            InvalidOperationException or ArgumentException => (StatusCodes.Status400BadRequest, exception.Message),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Accès non autorisé."),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Ressource introuvable."),
            _ => (StatusCodes.Status500InternalServerError, "Une erreur interne est survenue.")
        };

        httpContext.Response.StatusCode = statusCode;

        var response = ApiResponse.Fail(message, new List<string> { exception.Message });

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
