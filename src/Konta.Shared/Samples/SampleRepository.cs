using Konta.Shared.Data;
using Konta.Shared.Data.Repositories;
using Microsoft.Extensions.Logging;
using Dapper;

namespace Konta.Shared.Samples;

/// <summary>
/// Exemple de dépôt pour un futur microservice (ex: Compta, Stock, etc.)
/// montrant l'utilisation du BaseRepository et du SqlDebugHelper.
/// </summary>
public class SampleRepository : BaseRepository<SampleRepository>
{
    public SampleRepository(IDbConnectionFactory connectionFactory, ILogger<SampleRepository> logger) 
        : base(connectionFactory, logger)
    {
    }

    public async Task<Guid> CreateSampleAsync(object entity)
    {
        const string sql = "INSERT INTO Samples (Id, Name) VALUES (@Id, @Name) RETURNING Id";
        
        // Utilisation du BaseRepository qui gère automatiquement le log du SQL lisible
        using var connection = CreateConnection(sql, entity);
        
        return await connection.ExecuteScalarAsync<Guid>(sql, entity);
    }
}
