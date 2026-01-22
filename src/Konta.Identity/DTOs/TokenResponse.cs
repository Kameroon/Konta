namespace Konta.Identity.DTOs;

/// <summary>
/// Réponse contenant le jeton d'accès généré après authentification.
/// </summary>
public record TokenResponse
{
    /// <summary>
    /// Jeton d'accès (JWT).
    /// </summary>
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// Jeton de rafraîchissement.
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>
    /// Date d'expiration du jeton d'accès.
    /// </summary>
    public DateTime Expiration { get; init; }

    /// <summary>
    /// Informations sur l'utilisateur connecté.
    /// </summary>
    public UserInfoResponse User { get; init; } = new();
}
