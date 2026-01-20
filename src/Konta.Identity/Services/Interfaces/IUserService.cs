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
}
