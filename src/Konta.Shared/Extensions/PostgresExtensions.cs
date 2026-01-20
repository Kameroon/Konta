using Npgsql;

namespace Konta.Shared.Extensions;

/// <summary>
/// Méthodes d'extension pour faciliter la gestion des erreurs PostgreSQL.
/// </summary>
public static class PostgresExtensions
{
    /// <summary>
    /// Vérifie si l'exception est une violation d'unicité (code 23505).
    /// </summary>
    /// <param name="ex">L'exception à vérifier.</param>
    /// <returns>True si c'est une violation d'unicité, sinon False.</returns>
    public static bool IsUniqueViolation(this Exception ex)
    {
        return ex is PostgresException { SqlState: "23505" };
    }

    /// <summary>
    /// Vérifie si l'exception est une violation de clé étrangère (code 23503).
    /// </summary>
    /// <param name="ex">L'exception à vérifier.</param>
    /// <returns>True si c'est une violation de clé étrangère, sinon False.</returns>
    public static bool IsForeignKeyViolation(this Exception ex)
    {
        return ex is PostgresException { SqlState: "23503" };
    }

    /// <summary>
    /// Tente de convertir une Exception en PostgresException.
    /// </summary>
    /// <param name="ex">L'exception d'origine.</param>
    /// <returns>La PostgresException ou null.</returns>
    public static PostgresException? AsPostgresException(this Exception ex)
    {
        return ex as PostgresException;
    }
}
