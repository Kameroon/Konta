using Konta.Shared.Data.Postgres;

namespace Konta.Shared.Responses;

/// <summary>
/// Réponse spécifique pour les erreurs de base de données PostgreSQL.
/// </summary>
public class PostgresErrorResult : ApiResponse
{
    /// <summary>
    /// Détails techniques de l'erreur PostgreSQL.
    /// </summary>
    public PostgresErrorDetails DatabaseError { get; set; }

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="PostgresErrorResult"/>.
    /// </summary>
    /// <param name="details">Les détails de l'erreur.</param>
    public PostgresErrorResult(PostgresErrorDetails details)
    {
        Success = false;
        Message = details.Message ?? "Une erreur de base de données est survenue.";
        DatabaseError = details;
        Errors = new List<string> { details.Message ?? "Erreur DB" };
    }
}
