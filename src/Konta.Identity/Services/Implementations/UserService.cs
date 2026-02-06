using Konta.Identity.Data.Repositories.Interfaces;
using Konta.Identity.Models;
using Konta.Identity.Services.Interfaces;
using Konta.Shared.Services.Postgres;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Konta.Identity.Services.Implementations;

/// <summary>
/// Implémentation du service responsable de la gestion des utilisateurs.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly PostgresErrorService _errorService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository, 
        PostgresErrorService errorService,
        IPasswordHasher passwordHasher,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _errorService = errorService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Guid> CreateUserAsync(User user)
    {
        _logger.LogInformation("Début de la création de l'utilisateur : {Email}", user.Email);

        try
        {
            // Hachage du mot de passe avant insertion
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                user.PasswordHash = _passwordHasher.Hash(user.PasswordHash);
            }

            // Création de l'utilisateur en base
            // La validation d'unicité est gérée par la base de données via les contraintes
            var userId = await _userRepository.CreateAsync(user);
            
            _logger.LogInformation("Utilisateur créé avec succès. ID : {UserId}", userId);
            return userId;
        }
        catch (PostgresException ex)
        {
            var diagnosis = _errorService.Diagnose(ex);
            _logger.LogWarning("Erreur lors de la création de l'utilisateur {Email} : {Diagnosis}", user.Email, diagnosis.Message);
            throw; // Re-throw pour que PostgresExceptionHandler la capture
        }
    }

    /// <inheritdoc />
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        _logger.LogDebug("Recherche de l'utilisateur par email : {Email}", email);
        return await _userRepository.GetByEmailAsync(email);
    }

    /// <inheritdoc />
    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        _logger.LogDebug("Recherche de l'utilisateur par ID : {Id}", id);
        return await _userRepository.GetByIdAsync(id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetAllUsersByTenantIdAsync(Guid tenantId)
    {
        _logger.LogInformation("Récupération de tous les utilisateurs pour le tenant : {TenantId}", tenantId);
        return await _userRepository.GetAllByTenantIdAsync(tenantId);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        _logger.LogInformation("Récupération de TOUS les utilisateurs du système");
        return await _userRepository.GetAllAsync();
    }

    /// <inheritdoc />
    public async Task<bool> UpdateUserAsync(User user)
    {
        _logger.LogInformation("Mise à jour de l'utilisateur : {Id}", user.Id);
        return await _userRepository.UpdateAsync(user);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteUserAsync(Guid id)
    {
        _logger.LogInformation("Suppression de l'utilisateur : {Id}", id);
        return await _userRepository.DeleteAsync(id);
    }
}
