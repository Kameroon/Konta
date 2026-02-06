using Dapper;
using Konta.Shared.Data;
using Konta.Identity.Services.Interfaces;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace Konta.Identity.Data;

/// <summary>
/// Initialise la base de données Identity et crée les SuperAdmins par défaut.
/// </summary>
public class DatabaseInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly IPasswordHasher _passwordHasher;

    public DatabaseInitializer(
        IDbConnectionFactory connectionFactory,
        ILogger<DatabaseInitializer> logger,
        IWebHostEnvironment environment,
        IPasswordHasher passwordHasher)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        _environment = environment;
        _passwordHasher = passwordHasher;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Démarrage de l'initialisation de la base de données Identity...");

        try
        {
            // 1. Exécute le script SQL de création des tables et nettoyage des triggers
            await ExecuteSqlScriptAsync();
            
            // 2. Crée le tenant système et les SuperAdmins
            await SeedSuperAdminsAsync();

            _logger.LogInformation("Initialisation de la base de données Identity terminée avec succès.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'initialisation de la base de données Identity.");
        }
    }

    private async Task ExecuteSqlScriptAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        // 1. Nettoyage des anciens triggers qui causent des erreurs internes sur Azure
        _logger.LogInformation("Nettoyage des anciens triggers PL/pgSQL...");
        const string dropTriggersSql = @"
            DROP TRIGGER IF EXISTS update_tenants_modtime ON identity.Tenants;
            DROP TRIGGER IF EXISTS update_users_modtime ON identity.Users;
            DROP TRIGGER IF EXISTS update_roles_modtime ON identity.Roles;
            DROP TRIGGER IF EXISTS update_permissions_modtime ON identity.Permissions;
            DROP TRIGGER IF EXISTS update_refreshtokens_modtime ON identity.RefreshTokens;
            DROP FUNCTION IF EXISTS update_updated_at_column();";
        
        try {
            await connection.ExecuteAsync(dropTriggersSql);
        } catch (Exception ex) { 
            _logger.LogDebug("Impossible de supprimer les triggers (probablement tables inexistantes) : {Message}", ex.Message);
        }

        // 2. Exécution du script principal
        var scriptPath = Path.Combine(_environment.ContentRootPath, "Data", "Scripts", "init.sql");
        if (!File.Exists(scriptPath))
        {
            _logger.LogWarning("Script d'initialisation introuvable : {Path}", scriptPath);
            return;
        }

        var sql = await File.ReadAllTextAsync(scriptPath);
        await connection.ExecuteAsync(sql);
        
        _logger.LogInformation("Script SQL d'initialisation exécuté avec succès.");
    }

    private async Task SeedSuperAdminsAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        // Vérifie si le tenant système existe déjà
        const string checkTenantSql = "SELECT id FROM identity.tenants WHERE name = 'Konta Platform'";
        var existingTenantId = await connection.ExecuteScalarAsync<Guid?>(checkTenantSql);

        Guid systemTenantId;

        if (existingTenantId.HasValue)
        {
            systemTenantId = existingTenantId.Value;
            _logger.LogInformation("Tenant système trouvé : {TenantId}", systemTenantId);
        }
        else
        {
            // Crée le tenant système
            systemTenantId = Guid.NewGuid();
            const string insertTenantSql = @"
                INSERT INTO identity.tenants (id, name, identifier, plan, isactive, createdat)
                VALUES (@Id, @Name, @Identifier, @Plan, true, NOW())";
            
            await connection.ExecuteAsync(insertTenantSql, new 
            { 
                Id = systemTenantId, 
                Name = "Konta Platform", 
                Identifier = "konta-platform",
                Plan = "expertise"
            });
            _logger.LogInformation("Tenant système créé : {TenantId}", systemTenantId);
        }

        // Liste des SuperAdmins à créer/promouvoir
        var superAdmins = new[]
        {
            new { Email = "admin@konta.fr", FirstName = "Admin", LastName = "Konta", Password = "Admin123!" },
            new { Email = "support@konta.fr", FirstName = "Support", LastName = "Konta", Password = "Support123!" }
        };

        const string checkUserSql = "SELECT COUNT(*) FROM identity.users WHERE LOWER(email) = LOWER(@Email)";
        const string updateToSuperAdminSql = "UPDATE identity.users SET role = 'SuperAdmin' WHERE LOWER(email) = LOWER(@Email)";
        const string updateToSuperAdminSqlQuoted = "UPDATE identity.\"Users\" SET \"Role\" = 'SuperAdmin' WHERE LOWER(\"Email\") = LOWER(@Email)";
        const string getUserSql = "SELECT id, role FROM identity.users WHERE LOWER(email) = LOWER(@Email) LIMIT 1";
        const string insertUserSql = @"
            INSERT INTO identity.users (id, tenantid, email, passwordhash, firstname, lastname, role, isactive, createdat)
            VALUES (@Id, @TenantId, @Email, @PasswordHash, @FirstName, @LastName, 'SuperAdmin', true, NOW())";

        foreach (var admin in superAdmins)
        {
            _logger.LogInformation("Vérification de l'utilisateur : {Email}", admin.Email);
            var existingUser = await connection.QueryFirstOrDefaultAsync<UserSeedDto>(getUserSql, new { admin.Email });

            if (existingUser != null)
            {
                // Mise à jour du rôle vers SuperAdmin si l'utilisateur existe
                _logger.LogInformation("Utilisateur {Email} trouvé (Id: {Id}, Role actuel: {Role}). Mise à jour vers SuperAdmin...", admin.Email, existingUser.Id, existingUser.Role);
                var rowsAffected = await connection.ExecuteAsync(updateToSuperAdminSql, new { admin.Email });
                await connection.ExecuteAsync(updateToSuperAdminSqlQuoted, new { admin.Email });
                _logger.LogInformation("Utilisateur {Email} promu SuperAdmin. Lignes affectées : {Rows}", admin.Email, rowsAffected);
            }
            else
            {
                // Création d'un nouvel utilisateur SuperAdmin
                _logger.LogInformation("Utilisateur {Email} non trouvé. Création...", admin.Email);
                var userId = Guid.NewGuid();
                var passwordHash = _passwordHasher.Hash(admin.Password);

                await connection.ExecuteAsync(insertUserSql, new
                {
                    Id = userId,
                    TenantId = systemTenantId,
                    Email = admin.Email,
                    PasswordHash = passwordHash,
                    FirstName = admin.FirstName,
                    LastName = admin.LastName
                });

                _logger.LogInformation("SuperAdmin créé : {Email}", admin.Email);
            }
        }

        _logger.LogInformation("Initialisation des SuperAdmins terminée.");
    }

    private class UserSeedDto
    {
        public Guid Id { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
