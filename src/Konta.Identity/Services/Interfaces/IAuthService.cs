using Konta.Identity.DTOs;

namespace Konta.Identity.Services.Interfaces;

/// <summary>
/// Service responsable de l'authentification et de l'enregistrement des tenants.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Enregistre une nouvelle entreprise et son premier administrateur.
    /// </summary>
    Task<Guid> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Authentifie un utilisateur et retourne les tokens.
    /// </summary>
    Task<TokenResponse> LoginAsync(LoginRequest request);



    /// <summary>
    /// Rafraîchit les tokens d'accès.
    /// </summary>
    Task<TokenResponse> RefreshTokenAsync(string token, string refreshToken);
}
