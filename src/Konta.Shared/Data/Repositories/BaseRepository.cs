using System.Data;
using Dapper;
using Konta.Shared.Data.Helpers;
using Microsoft.Extensions.Logging;

namespace Konta.Shared.Data.Repositories;

/// <summary>
/// Classe de base pour les dépôts de données, fournissant des outils de debug et d'accès.
/// </summary>
/// <typeparam name="TRepository">Le type du dépôt (pour le logging).</typeparam>
public abstract class BaseRepository<TRepository>
{
    protected readonly IDbConnectionFactory ConnectionFactory;
    protected readonly ILogger<TRepository> _logger;

    protected BaseRepository(IDbConnectionFactory connectionFactory, ILogger<TRepository> logger)
    {
        ConnectionFactory = connectionFactory;
        _logger = logger;
    }

    /// <summary>
    /// Crée une connexion et log la version lisible de la requête SQL si le niveau Debug est activé.
    /// </summary>
    /// <param name="sql">La requête SQL.</param>
    /// <param name="parameters">Les paramètres de la requête.</param>
    /// <returns>Une instance de IDbConnection.</returns>
    protected IDbConnection CreateConnection(string sql, object? parameters = null)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var readableSql = SqlDebugHelper.GetReadableSql(sql, parameters);
            _logger.LogDebug("DEBUG SQL EXECUTION:\n{Query}", readableSql);
        }

        return ConnectionFactory.CreateConnection();
    }
}
