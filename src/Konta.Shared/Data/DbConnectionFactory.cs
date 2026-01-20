using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Konta.Shared.Data;

/// <summary>
/// Implémentation de la fabrique de connexions pour PostgreSQL.
/// </summary>
public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;
    private readonly ITenantContext _tenantContext;

    public DbConnectionFactory(IConfiguration configuration, ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "La chaîne de connexion 'DefaultConnection' est manquante.");
    }

    public IDbConnection CreateConnection()
    {
        var connection = new NpgsqlConnection(_connectionString);
        
        // Si un TenantId est présent, on le prépare pour la session
        if (_tenantContext.TenantId.HasValue)
        {
            connection.StateChange += (s, e) =>
            {
                if (e.CurrentState == ConnectionState.Open)
                {
                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = $"SET app.current_tenant_id = '{_tenantContext.TenantId}';";
                    cmd.ExecuteNonQuery();
                }
            };
        }

        return connection;
    }
}
