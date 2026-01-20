namespace Konta.Shared.Services.Postgres;

/// <summary>
/// Options pour la configuration de la gestion des erreurs PostgreSQL.
/// </summary>
public class PostgresErrorOptions
{
    /// <summary>
    /// Dictionnaire associant le nom d'une contrainte à son message d'erreur localisé.
    /// Clé : Nom de la contrainte (ex: "IX_Users_Email").
    /// Valeur : Message d'erreur (ex: "Cette adresse email est déjà utilisée.").
    /// </summary>
    public Dictionary<string, string> UniqueViolations { get; set; } = new();

    /// <summary>
    /// Message par défaut pour les violations d'unicité non répertoriées.
    /// </summary>
    public string DefaultUniqueViolationMessage { get; set; } = "Une donnée identique existe déjà dans le système.";

    /// <summary>
    /// Message par défaut pour les violations de clés étrangères.
    /// </summary>
    public string DefaultForeignKeyViolationMessage { get; set; } = "Cette opération ne peut pas être effectuée car elle est liée à d'autres données existantes.";
}
