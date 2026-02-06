using Konta.Tenant.DTOs;

namespace Konta.Tenant.Services.Interfaces;

/// <summary>
/// Service gérant les opérations liées aux entreprises (Tenants).
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Crée une nouvelle entreprise et lui affecte un plan par défaut.
    /// </summary>
    Task<TenantResponse> CreateTenantAsync(CreateTenantRequest request);

    /// <summary>
    /// Récupère les détails d'une entreprise par son ID.
    /// </summary>
    Task<TenantResponse?> GetTenantAsync(Guid id);

    /// <summary>
    /// Récupère la liste de toutes les entreprises.
    /// </summary>
    Task<IEnumerable<TenantResponse>> GetAllTenantsAsync();

    /// <summary>
    /// Recherche une entreprise par son identifiant unique (SIRET ou slug).
    /// </summary>
    Task<TenantResponse?> GetTenantByIdentifierAsync(string identifier);

    /// <summary>
    /// Met à jour les informations d'une entreprise existante.
    /// </summary>
    Task<bool> UpdateTenantAsync(Guid id, UpdateTenantRequest request);
}
