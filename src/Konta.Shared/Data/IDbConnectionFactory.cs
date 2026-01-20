using System.Data;

namespace Konta.Shared.Data;

/// <summary>
/// Interface définissant la fabrique de connexions à la base de données.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Crée une nouvelle instance de connexion vers PostgreSQL.
    /// </summary>
    /// <returns>Une instance de IDbConnection prête à l'emploi.</returns>
    IDbConnection CreateConnection(); // Méthode de création de connexion
}
