using System.ComponentModel.DataAnnotations;

namespace Konta.Identity.DTOs;

/// <summary>
/// Requête pour rafraîchir un jeton d'accès expiré.
/// </summary>
public record RefreshTokenRequest
{
    /// <summary>
    /// Jeton d'accès (JWT) expiré ou sur le point d'expirer.
    /// </summary>
    [Required(ErrorMessage = "Le jeton d'accès est obligatoire.")]
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// Jeton de rafraîchissement associé à la session.
    /// </summary>
    [Required(ErrorMessage = "Le jeton de rafraîchissement est obligatoire.")]
    public string RefreshToken { get; init; } = string.Empty;
}
