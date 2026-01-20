using System.ComponentModel.DataAnnotations;

namespace Konta.Identity.DTOs;

/// <summary>
/// Requête de connexion pour obtenir un jeton d'accès.
/// </summary>
public record LoginRequest
{
    /// <summary>
    /// Adresse email de l'utilisateur.
    /// </summary>
    [Required(ErrorMessage = "L'adresse email est obligatoire.")]
    [EmailAddress(ErrorMessage = "Format d'email invalide.")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Mot de passe de l'utilisateur.
    /// </summary>
    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    public string Password { get; init; } = string.Empty;
}
