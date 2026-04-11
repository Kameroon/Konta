using Konta.Tenant.Data.Repositories.Interfaces; // Accès aux dépôts
using Konta.Tenant.DTOs; // Objets de transfert de données
using Konta.Tenant.Models; // Modèles du domaine
using Konta.Tenant.Services.Interfaces; // Définition du service
using Konta.Shared.Services.Postgres; // Service d'erreur partagé
using Npgsql; // Client Postgres
using Microsoft.Extensions.Logging;

namespace Konta.Tenant.Services.Implementations; // Couche de logique métier

/// <summary>
/// Service gérant les opérations liées aux entreprises (Tenants).
/// </summary>
public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepository; // Dépôt des tenants
    private readonly PostgresErrorService _errorService; // Service d'erreur Postgres
    private readonly ILogger<TenantService> _logger; // Service de journalisation

    /// <summary>
    /// Constructeur avec injection des dépendances.
    /// </summary>
    public TenantService(
        ITenantRepository tenantRepository, 
        PostgresErrorService errorService,
        ILogger<TenantService> logger)
    {
        _tenantRepository = tenantRepository; // Initialisation du dépôt
        _errorService = errorService; // Initialisation du service d'erreur
        _logger = logger; // Initialisation du logger
    }

    /// <inheritdoc />
    public async Task<TenantResponse> CreateTenantAsync(CreateTenantRequest request)
    {
        // Journalisation du début de création
        _logger.LogInformation("Lancement de la création du tenant {Identifier}", request.Identifier);
        
        try
        {
            // Mappage de la requête vers l'entité du domaine
            var tenant = new Models.Tenant
            {
                Name = request.Name, // Nom de la société
                Identifier = request.Identifier, // Slug/Domaine unique
                Industry = request.Industry, // Secteur d'activité
                Address = request.Address, // Adresse physique
                Siret = request.Siret // Numéro SIRET
            };

            // Persistance du tenant dans la base de données
            var id = await _tenantRepository.CreateAsync(tenant);
            
            _logger.LogInformation("Tenant {Id} créé avec succès. La souscription est désormais gérée par le microservice Billing.", id);

            // Retourne la réponse formatée
            return new TenantResponse
            {
                Id = id,
                Name = tenant.Name,
                Identifier = tenant.Identifier,
                Industry = tenant.Industry ?? "",
                CreatedAt = tenant.CreatedAt,
                IsActive = true,
                Plan = tenant.Plan
            };
        }
        catch (PostgresException ex)
        {
            var diagnosis = _errorService.Diagnose(ex);
            _logger.LogWarning("Erreur lors de la création du tenant {Identifier} : {Diagnosis}", request.Identifier, diagnosis.Message);
            throw; // Re-throw pour que PostgresExceptionHandler la capture
        }
    }

    /// <inheritdoc />
    public async Task<TenantResponse?> GetTenantAsync(Guid id)
    {
        // Recherche du tenant par son identifiant unique
        _logger.LogDebug("Récupération des détails pour le tenant {Id}", id);
        var tenant = await _tenantRepository.GetByIdAsync(id);
        
        // Gestion du cas où le tenant n'existe pas
        if (tenant == null) return null;
        
        // Conversion de l'entité vers le DTO de réponse
        return new TenantResponse
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Identifier = tenant.Identifier,
            Industry = tenant.Industry ?? "",
            CreatedAt = tenant.CreatedAt,
            IsActive = tenant.IsActive,
            Plan = tenant.Plan
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TenantResponse>> GetAllTenantsAsync()
    {
        // Récupération de la liste complète des entreprises enregistrées
        _logger.LogInformation("Récupération de la liste complète des tenants");
        var tenants = await _tenantRepository.GetAllAsync();
        
        // Mappage de la collection vers une liste de DTOs
        return tenants.Select(t => new TenantResponse
        {
            Id = t.Id,
            Name = t.Name,
            Identifier = t.Identifier,
            Industry = t.Industry ?? "",
            CreatedAt = t.CreatedAt,
            IsActive = t.IsActive,
            Plan = t.Plan
        });
    }

    /// <inheritdoc />
    public async Task<TenantResponse?> GetTenantByIdentifierAsync(string identifier)
    {
        // Recherche du tenant par son SIRET
        _logger.LogDebug("Recherche du tenant par SIRET : {Siret}", identifier);
        var tenant = await _tenantRepository.GetBySiretAsync(identifier);
        
        // Si non trouvé par SIRET, essayer par Identifier (slug)
        if (tenant == null)
        {
            _logger.LogDebug("SIRET non trouvé, recherche par identifiant : {Identifier}", identifier);
            tenant = await _tenantRepository.GetByIdentifierAsync(identifier);
        }
        
        // Gestion du cas où le tenant n'existe pas
        if (tenant == null) return null;
        
        // Conversion de l'entité vers le DTO de réponse
        return new TenantResponse
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Identifier = tenant.Identifier,
            Industry = tenant.Industry ?? "",
            CreatedAt = tenant.CreatedAt,
            IsActive = tenant.IsActive,
            Plan = tenant.Plan
        };
    }

    /// <inheritdoc />
    public async Task<bool> UpdateTenantAsync(Guid id, UpdateTenantRequest request)
    {
        _logger.LogInformation("Mise à jour du tenant {Id}", id);
        
        // 1. Récupération du tenant existant
        var existing = await _tenantRepository.GetByIdAsync(id);
        if (existing == null)
        {
            _logger.LogWarning("Tenant {Id} introuvable pour mise à jour.", id);
            return false;
        }

        // 2. Mise à jour des propriétés
        existing.Name = request.Name;
        existing.Industry = request.Industry;
        existing.Address = request.Address;
        existing.Siret = request.Siret;
        existing.Plan = request.Plan;
        existing.IsActive = request.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        // 3. Persistance
        return await _tenantRepository.UpdateAsync(existing);
    }

    /// <inheritdoc />
    public async Task<bool> ActivateAccessAsync(Guid tenantId)
    {
        _logger.LogInformation("Activation de l'accès pour le tenant {Id}", tenantId);
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant == null) return false;

        tenant.IsActive = true;
        tenant.UpdatedAt = DateTime.UtcNow;
        return await _tenantRepository.UpdateAsync(tenant);
    }

    /// <inheritdoc />
    public async Task<bool> DeactivateAccessAsync(Guid tenantId)
    {
        _logger.LogWarning("Désactivation de l'accès pour le tenant {Id}", tenantId);
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant == null) return false;

        tenant.IsActive = false;
        tenant.UpdatedAt = DateTime.UtcNow;
        return await _tenantRepository.UpdateAsync(tenant);
    }
}
