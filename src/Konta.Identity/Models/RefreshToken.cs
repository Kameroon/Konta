using Konta.Shared.Models;

namespace Konta.Identity.Models;

/// <summary>
/// Représente un jeton de rafraîchissement pour la session utilisateur.
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>
    /// Identifiant de l'utilisateur associé au jeton.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Valeur unique du jeton de rafraîchissement.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Date et heure d'expiration du jeton.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Indique si le jeton a été révoqué manuellement.
    /// </summary>
    public bool IsRevoked { get; set; } = false;

    /// <summary>
    /// Nouveau jeton ayant remplacé celui-ci après un rafraîchissement.
    /// </summary>
    public string ReplacedByToken { get; set; } = string.Empty;
}
