using Konta.Tenant.Models;

namespace Konta.Tenant.Data.Repositories.Interfaces;

/// <summary>
/// Définit les opérations de persistance pour les entreprises (Tenants).
/// </summary>
public interface ITenantRepository
{
    /// <summary>
    /// Récupère un tenant par son identifiant unique global.
    /// </summary>
    Task<Models.Tenant?> GetByIdAsync(Guid id);

    /// <summary>
    /// Récupère un tenant par son identifiant textuel (slug).
    /// </summary>
    Task<Models.Tenant?> GetByIdentifierAsync(string identifier);

    /// <summary>
    /// Récupère un tenant par son numéro SIRET.
    /// </summary>
    Task<Models.Tenant?> GetBySiretAsync(string siret);

    /// <summary>
    /// Récupère la liste de tous les tenants enregistrés.
    /// </summary>
    Task<IEnumerable<Models.Tenant>> GetAllAsync();

    /// <summary>
    /// Enregistre une nouvelle entreprise en base de données.
    /// </summary>
    Task<Guid> CreateAsync(Models.Tenant tenant);

    /// <summary>
    /// Met à jour les informations d'une entreprise existante.
    /// </summary>
    Task<bool> UpdateAsync(Models.Tenant tenant);

    /// <summary>
    /// Supprime définitivement une entreprise de la base de données.
    /// </summary>
    Task<bool> DeleteAsync(Guid id);
}
