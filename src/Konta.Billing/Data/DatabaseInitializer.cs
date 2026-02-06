using Dapper;
using Konta.Shared.Data;
using System.IO;

namespace Konta.Billing.Data;

public class DatabaseInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly IWebHostEnvironment _environment;

    public DatabaseInitializer(IDbConnectionFactory connectionFactory, ILogger<DatabaseInitializer> logger, IWebHostEnvironment environment)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        _environment = environment;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Démarrage de l'initialisation de la base de données Billing...");

        try
        {
            var scriptPath = Path.Combine(_environment.ContentRootPath, "Data", "Scripts", "Init_Billing.sql");
            
            if (!File.Exists(scriptPath))
            {
                _logger.LogWarning("Script d'initialisation introuvable : {Path}", scriptPath);
                return;
            }

            var sql = await File.ReadAllTextAsync(scriptPath);
            
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql);

            _logger.LogInformation("Initialisation de la base de données Billing terminée avec succès.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'initialisation de la base de données Billing.");
            // On ne bloque pas le démarrage de l'app mais on logge l'erreur critique
        }
    }
}
