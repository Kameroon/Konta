using System.ComponentModel.DataAnnotations;

namespace Konta.Identity.DTOs;

/// <summary>
/// Requête pour l'enregistrement d'une nouvelle entreprise et de son premier utilisateur administrateur.
/// </summary>
public record RegisterRequest
{
    /// <summary>
    /// Nom de l'entreprise à créer.
    /// </summary>
    [Required(ErrorMessage = "Le nom de l'entreprise est obligatoire.")]
    public string TenantName { get; init; } = string.Empty;

    /// <summary>
    /// Adresse email du futur administrateur.
    /// </summary>
    [Required(ErrorMessage = "L'adresse email est obligatoire.")]
    [EmailAddress(ErrorMessage = "Format d'email invalide.")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Mot de passe du compte administrateur.
    /// </summary>
    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Prénom de l'administrateur.
    /// </summary>
    [Required(ErrorMessage = "Le prénom est obligatoire.")]
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Nom de famille de l'administrateur.
    /// </summary>
    [Required(ErrorMessage = "Le nom de famille est obligatoire.")]
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Plan d'abonnement sélectionné (ex: discovery, basic, advanced, premium, expertise).
    /// </summary>
    [Required(ErrorMessage = "Le choix d'un forfait est obligatoire.")]
    public string Plan { get; init; } = "discovery";
}
