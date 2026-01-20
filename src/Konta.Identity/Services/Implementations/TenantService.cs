using Konta.Identity.Data.Repositories.Interfaces;
using Konta.Identity.DTOs;
using Konta.Identity.Models;
using Konta.Identity.Services.Interfaces;
using Konta.Shared.Services.Postgres;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Konta.Identity.Services.Implementations;

/// <summary>
/// Implémentation du service de gestion des tenants.
/// </summary>
public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly PostgresErrorService _errorService;
    private readonly ILogger<TenantService> _logger;

    public TenantService(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IPasswordHasher passwordHasher,
        PostgresErrorService errorService,
        ILogger<TenantService> logger)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _passwordHasher = passwordHasher;
        _errorService = errorService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Guid> RegisterTenantAsync(RegisterRequest request)
    {
        _logger.LogInformation("Début de l'inscription pour le tenant : {TenantName}", request.TenantName);

        try
        {
            // 1. Création du Tenant
            _logger.LogDebug("Création du tenant : {TenantName}", request.TenantName);
            var tenant = new Tenant
            {
                Name = request.TenantName
            };
            var tenantId = await _tenantRepository.CreateAsync(tenant);

        // 3. Création du Rôle Admin par défaut
        _logger.LogDebug("Création du rôle Admin pour le tenant ID : {TenantId}", tenantId);
        var adminRole = new Role
        {
            TenantId = tenantId,
            Name = "Admin",
            Description = "Administrateur avec accès complet",
            IsDefault = true
        };
        var adminRoleId = await _roleRepository.CreateAsync(adminRole);

        // 4. Création de l'utilisateur Admin
        _logger.LogDebug("Création de l'utilisateur Admin : {Email}", request.Email);
        var user = new User
        {
            TenantId = tenantId,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = "Admin",
            IsActive = true
        };
        // Appel direct au Repository pour la persistance, car UserService peut avoir des règles métier spécifiques
        // Ou utiliser IUserService si on veut passer par le service. Ici on veut juste créer l'admin initial.
        var userId = await _userRepository.CreateAsync(user);

        // 5. Assignation du Rôle Admin
        _logger.LogDebug("Assignation du rôle Admin à l'utilisateur ID : {UserId}", userId);
        await _roleRepository.AssignRoleToUserAsync(userId, adminRoleId);

        // 6. Assignation des Permissions par défaut
        _logger.LogDebug("Attribution des permissions par défaut au rôle Admin");
        var defaultPermissions = new[] { "users.read", "users.write", "roles.read", "roles.write" };
        foreach (var permName in defaultPermissions)
        {
            var perm = await _permissionRepository.GetBySystemNameAsync(permName);
            if (perm != null)
            {
                await _roleRepository.AddPermissionToRoleAsync(adminRoleId, perm.Id);
            }
        }

        _logger.LogInformation("Inscription terminée avec succès pour le tenant : {TenantName}", request.TenantName);
        return userId;
    }
    catch (PostgresException ex)
    {
        var diagnosis = _errorService.Diagnose(ex);
        _logger.LogWarning("Erreur lors de l'inscription du tenant {TenantName} : {Diagnosis}", request.TenantName, diagnosis.Message);
        throw new InvalidOperationException(diagnosis.Message);
    }
}

    /// <inheritdoc />
    public async Task<TenantResponse?> GetTenantByIdAsync(Guid id)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);
        return tenant == null ? null : MapToResponse(tenant);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TenantResponse>> GetAllTenantsAsync()
    {
        var tenants = await _tenantRepository.GetAllAsync();
        return tenants.Select(MapToResponse);
    }

    /// <inheritdoc />
    public async Task<bool> UpdateTenantAsync(Guid id, UpdateTenantRequest request)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);
        if (tenant == null) return false;

        tenant.Name = request.Name;
        tenant.Plan = request.Plan;
        tenant.UpdatedAt = DateTime.UtcNow;

        return await _tenantRepository.UpdateAsync(tenant);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteTenantAsync(Guid id)
    {
        return await _tenantRepository.DeleteAsync(id);
    }

    private static TenantResponse MapToResponse(Tenant tenant)
    {
        return new TenantResponse
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Plan = tenant.Plan,
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt
        };
    }
}
