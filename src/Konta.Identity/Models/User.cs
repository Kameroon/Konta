using Konta.Shared.Models;

namespace Konta.Identity.Models;

/// <summary>
/// Représente un utilisateur dans le système.
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Identifiant du tenant auquel l'utilisateur appartient.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Adresse email unique de l'utilisateur.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Empreinte du mot de passe sécurisée.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Prénom de l'utilisateur.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Nom de famille de l'utilisateur.
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Rôle principal de l'utilisateur.
    /// </summary>
    public string Role { get; set; } = "User"; 
}
