using Dapper; // Micro-ORM pour l'accès aux données
using Konta.Tenant.Data.Repositories.Interfaces; // Interface de définition
using Konta.Shared.Data; // Utilisation de l'interface factory partagée
using Konta.Shared.Data.Repositories; // Classe de base des dépôts
using Konta.Tenant.Models; // Modèles de données
using System.Data; // Manipulation de données SQL

namespace Konta.Tenant.Data.Repositories.Implementations; // Couche implémentation

/// <summary>
/// Dépôt gérant la persistance des entreprises (Tenants).
/// </summary>
public class TenantRepository : BaseRepository<TenantRepository>, ITenantRepository
{
    /// <summary>
    /// Initialise une nouvelle instance du dépôt.
    /// </summary>
    public TenantRepository(IDbConnectionFactory connectionFactory, ILogger<TenantRepository> logger)
        : base(connectionFactory, logger)
    {
    }

    /// <inheritdoc />
    public async Task<Models.Tenant?> GetByIdAsync(Guid id)
    {
        _logger.LogDebug("Accès DB : Récupération du tenant par ID {Id}", id); // Log de débogage
        const string sql = "SELECT * FROM Tenants WHERE Id = @Id";
        using var connection = CreateConnection(sql, new { Id = id }); 
        return await connection.QuerySingleOrDefaultAsync<Models.Tenant>(sql, new { Id = id }); 
    }

    /// <inheritdoc />
    public async Task<Models.Tenant?> GetByIdentifierAsync(string identifier)
    {
        _logger.LogDebug("Accès DB : Récupération du tenant par identifiant {Identifier}", identifier); // Log de débogage
        const string sql = "SELECT * FROM Tenants WHERE Identifier = @Identifier";
        using var connection = CreateConnection(sql, new { Identifier = identifier });
        return await connection.QuerySingleOrDefaultAsync<Models.Tenant>(sql, new { Identifier = identifier });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Models.Tenant>> GetAllAsync()
    {
        _logger.LogDebug("Accès DB : Récupération de tous les tenants"); // Log de débogage
        const string sql = "SELECT * FROM Tenants ORDER BY CreatedAt DESC";
        using var connection = CreateConnection(sql);
        return await connection.QueryAsync<Models.Tenant>(sql);
    }

    /// <inheritdoc />
    public async Task<Guid> CreateAsync(Models.Tenant tenant)
    {
        _logger.LogInformation("Accès DB : Insertion du nouveau tenant {Name}", tenant.Name); // Log d'information
        const string sql = @"
            INSERT INTO Tenants (Id, Name, Identifier, Industry, Address, TaxId, CreatedAt)
            VALUES (@Id, @Name, @Identifier, @Industry, @Address, @TaxId, @CreatedAt)
            RETURNING Id"; // Requête SQL brute avec retour de l'ID
        
        using var connection = CreateConnection(sql, tenant);
        return await connection.ExecuteScalarAsync<Guid>(sql, tenant); // Exécution et retour de l'ID généré
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(Models.Tenant tenant)
    {
        _logger.LogInformation("Accès DB : Mise à jour du tenant {Id}", tenant.Id); // Log d'information
        const string sql = @"
            UPDATE Tenants 
            SET Name = @Name, 
                Identifier = @Identifier, 
                Industry = @Industry, 
                Address = @Address, 
                TaxId = @TaxId, 
                UpdatedAt = @UpdatedAt,
                IsActive = @IsActive
            WHERE Id = @Id"; // Mise à jour complète de l'entité
        
        using var connection = CreateConnection(sql, tenant);
        var rows = await connection.ExecuteAsync(sql, tenant); // Exécution de la requête
        return rows > 0; // Retourne vrai si au moins une ligne a été modifiée
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogWarning("Accès DB : Suppression (physique) du tenant {Id}", id); // Log d'avertissement
        const string sql = "DELETE FROM Tenants WHERE Id = @Id";
        using var connection = CreateConnection(sql, new { Id = id });
        var rows = await connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0; // Retourne vrai si la suppression a réussi
    }
}
