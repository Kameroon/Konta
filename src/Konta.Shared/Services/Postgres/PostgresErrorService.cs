using Npgsql;
using Konta.Shared.Data.Postgres;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Konta.Shared.Services.Postgres;

/// <summary>
/// Service de diagnostic et de traduction des erreurs PostgreSQL.
/// Ce service est générique et utilise des options pour la traduction spécifique.
/// </summary>
public class PostgresErrorService
{
    private readonly PostgresErrorOptions _options;
    private readonly ILogger<PostgresErrorService> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="PostgresErrorService"/>.
    /// </summary>
    /// <param name="options">Les options de configuration.</param>
    /// <param name="logger">Le logger.</param>
    public PostgresErrorService(IOptions<PostgresErrorOptions> options, ILogger<PostgresErrorService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Analyse une exception PostgreSQL et retourne un diagnostic structuré.
    /// </summary>
    /// <param name="ex">L'exception PostgreSQL.</param>
    /// <returns>Les détails de l'erreur avec un message traduit.</returns>
    public PostgresErrorDetails Diagnose(PostgresException ex)
    {
        var details = new PostgresErrorDetails
        {
            Code = ex.SqlState,
            ConstraintName = ex.ConstraintName,
            TableName = ex.TableName,
            Message = "Une erreur de base de données est survenue."
        };

        switch (ex.SqlState)
        {
            case "23505": // unique_violation
                details.Message = DiagnoseUniqueViolation(ex);
                break;
            case "23503": // foreign_key_violation
                details.Message = _options.DefaultForeignKeyViolationMessage;
                break;
            case "23502": // not_null_violation
                details.Message = $"Le champ '{ex.ColumnName}' ne peut pas être vide.";
                break;
            case "23514": // check_violation
                details.Message = "Une règle de validation de données n'a pas été respectée.";
                break;
            default:
                _logger.LogWarning("Erreur Postgres non diagnostiquée explicitement : {Code} - {Message}", ex.SqlState, ex.Message);
                break;
        }

        return details;
    }

    private string DiagnoseUniqueViolation(PostgresException ex)
    {
        if (ex.ConstraintName != null && _options.UniqueViolations.TryGetValue(ex.ConstraintName, out var message))
        {
            return message;
        }

        return _options.DefaultUniqueViolationMessage;
    }
}
