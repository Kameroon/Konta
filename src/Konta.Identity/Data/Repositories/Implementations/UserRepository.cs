using Dapper; // Micro-ORM pour l'accès aux données
using Konta.Identity.Data.Repositories.Interfaces; // Definition des contrats
using Konta.Identity.Models; // Modèles du domaine
using Microsoft.Extensions.Logging; // Journalisation
using Konta.Shared.Data; // Interface factory partagée
using Konta.Shared.Data.Repositories; // Classe de base des dépôts

namespace Konta.Identity.Data.Repositories.Implementations;

/// <summary>
/// Dépôt gérant la persistance des utilisateurs.
/// </summary>
public class UserRepository : BaseRepository<UserRepository>, IUserRepository
{
    /// <summary>
    /// Initialise le dépôt des utilisateurs.
    /// </summary>
    public UserRepository(IDbConnectionFactory connectionFactory, ILogger<UserRepository> logger)
        : base(connectionFactory, logger)
    {
    }

    /// <inheritdoc />
    public async Task<User?> GetByEmailAsync(string email)
    {
        _logger.LogDebug("Accès DB : Recherche utilisateur par email: {Email}", email);
        const string sql = "SELECT * FROM identity.Users WHERE Email = @Email";
        using var connection = CreateConnection(sql, new { Email = email });
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
    }

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(Guid id)
    {
        _logger.LogDebug("Accès DB : Recherche utilisateur par ID: {Id}", id);
        const string sql = "SELECT * FROM identity.Users WHERE Id = @Id";
        using var connection = CreateConnection(sql, new { Id = id });
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
    }

    /// <inheritdoc />
    public async Task<Guid> CreateAsync(User user)
    {
        _logger.LogInformation("Accès DB : Création utilisateur: {Email}", user.Email);
        const string sql = @"
            INSERT INTO identity.Users (Id, TenantId, Email, PasswordHash, FirstName, LastName, Role, IsActive, CreatedAt) 
            VALUES (@Id, @TenantId, @Email, @PasswordHash, @FirstName, @LastName, @Role, @IsActive, @CreatedAt)
            RETURNING Id";
        
        using var connection = CreateConnection(sql, user);
        return await connection.ExecuteScalarAsync<Guid>(sql, user);
    }
}
