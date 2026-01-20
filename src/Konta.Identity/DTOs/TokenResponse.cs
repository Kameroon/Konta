namespace Konta.Identity.DTOs;

/// <summary>
/// Réponse contenant le jeton d'accès généré après authentification.
/// </summary>
public record TokenResponse
{
    /// <summary>
    /// Jeton d'accès (JWT) à utiliser pour les requêtes authentifiées.
    /// </summary>
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// Date et heure d'expiration du jeton d'accès.
    /// </summary>
    public DateTime Expiration { get; init; }
}
