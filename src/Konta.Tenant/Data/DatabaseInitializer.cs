using Dapper;
using Konta.Shared.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Konta.Tenant.Data;

/// <summary>
/// Initialise la base de données pour le microservice de Tenant Management.
/// </summary>
public class DatabaseInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly IWebHostEnvironment _environment;

    public DatabaseInitializer(
        IDbConnectionFactory connectionFactory,
        ILogger<DatabaseInitializer> logger,
        IWebHostEnvironment environment)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        _environment = environment;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Démarrage de l'initialisation de la base de données Tenant...");

        try
        {
            await ExecuteSqlScriptAsync();
            _logger.LogInformation("Initialisation de la base de données Tenant terminée avec succès.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'initialisation de la base de données Tenant.");
        }
    }

    private async Task ExecuteSqlScriptAsync()
    {
        var scriptPath = Path.Combine(_environment.ContentRootPath, "Data", "Scripts", "init.sql");
        
        if (!File.Exists(scriptPath))
        {
            _logger.LogWarning("Script d'initialisation introuvable : {Path}", scriptPath);
            return;
        }

        var sql = await File.ReadAllTextAsync(scriptPath);
        
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql);
        
        _logger.LogInformation("Script SQL exécuté avec succès.");
    }
}
