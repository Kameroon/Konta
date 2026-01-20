namespace Konta.Shared.Data.Postgres;

/// <summary>
/// Représente les détails d'une erreur PostgreSQL de manière structurée.
/// </summary>
public class PostgresErrorDetails
{
    /// <summary>
    /// Code d'erreur PostgreSQL (ex: 23505 pour violation d'unicité).
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Nom de la contrainte ayant échoué.
    /// </summary>
    public string? ConstraintName { get; set; }

    /// <summary>
    /// Nom de la table concernée par l'erreur.
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>
    /// Message d'erreur diagnostiqué en Français.
    /// </summary>
    public string? Message { get; set; }
}
