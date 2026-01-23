using Konta.Identity.Models;

namespace Konta.Identity.Services.Interfaces;

/// <summary>
/// Service responsable de la gestion des utilisateurs.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Crée un nouvel utilisateur.
    /// </summary>
    Task<Guid> CreateUserAsync(User user);
    
    /// <summary>
    /// Récupère un utilisateur par son email.
    /// </summary>
    Task<User?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Récupère un utilisateur par son ID.
    /// </summary>
    Task<User?> GetUserByIdAsync(Guid id);

    /// <summary>
    /// Récupère tous les utilisateurs d'un tenant.
    /// </summary>
    Task<IEnumerable<User>> GetAllUsersByTenantIdAsync(Guid tenantId);
    Task<IEnumerable<User>> GetAllUsersAsync();

    /// <summary>
    /// Met à jour les informations d'un utilisateur.
    /// </summary>
    Task<bool> UpdateUserAsync(User user);

    /// <summary>
    /// Supprime un utilisateur par son ID.
    /// </summary>
    Task<bool> DeleteUserAsync(Guid id);
}
