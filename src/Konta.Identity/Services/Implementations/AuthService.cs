using Konta.Identity.Data.Repositories.Interfaces;
using Konta.Identity.DTOs;
using Konta.Identity.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Konta.Identity.Services.Implementations;

/// <summary>
/// Implémentation du service d'authentification.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserService userService,
        IPermissionRepository permissionRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _userService = userService;
        _permissionRepository = permissionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TokenResponse> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Tentative de connexion pour : {Email}", request.Email);

        // 1. Récupération de l'utilisateur via le service dédié
        _logger.LogInformation("[Step 1] Début récupération utilisateur");
        var user = await _userService.GetUserByEmailAsync(request.Email);
        _logger.LogInformation("[Step 1] Fin récupération utilisateur (Trouvé: {UserFound})", user != null);
        
        if (user == null)
        {
            _logger.LogWarning("Échec de connexion : Utilisateur introuvable pour l'email '{Email}'", request.Email);
            throw new UnauthorizedAccessException("Identifiants invalides");
        }

        // 2. Vérification des identifiants
        _logger.LogInformation("[Step 2] Début vérification mot de passe");
        bool isPasswordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);
        _logger.LogInformation("[Step 2] Fin vérification mot de passe (Valide: {IsValid})", isPasswordValid);
        
        if (!isPasswordValid)
        {
            _logger.LogWarning("Échec de connexion : Mot de passe incorrect pour l'email '{Email}'. Hash en base : {HashPrefix}...", request.Email, user.PasswordHash.Substring(0, Math.Min(10, user.PasswordHash.Length)));
            throw new UnauthorizedAccessException("Identifiants invalides");
        }

        // 3. Vérification du statut de l'utilisateur
        _logger.LogInformation("[Step 3] Vérification statut");
        if (!user.IsActive)
        {
            _logger.LogWarning("Tentative de connexion d'un utilisateur inactif : {Email}", request.Email);
            throw new UnauthorizedAccessException("Utilisateur inactif");
        }

        // 4. Récupération des permissions
        _logger.LogInformation("[Step 4] Début récupération permissions");
        
        IEnumerable<string> permissions;
        if (user.Role == "SuperAdmin")
        {
            _logger.LogInformation("Utilisateur SuperAdmin détecté. Attribution de TOUTES les permissions système.");
            permissions = await _permissionRepository.GetAllPermissionSystemNamesAsync();
        }
        else
        {
            permissions = await _permissionRepository.GetPermissionsByUserIdAsync(user.Id);
        }
        _logger.LogInformation("[Step 4] Fin récupération permissions (Compte: {Count})", permissions.Count());

        // 5. Génération des tokens
        _logger.LogInformation("[Step 5] Génération tokens");
        var accessToken = _tokenService.GenerateToken(user, user.TenantId.ToString(), permissions);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

        // 6. Sauvegarde du Refresh Token
        _logger.LogInformation("[Step 6] Sauvegarde Refresh Token");
        await _refreshTokenRepository.CreateAsync(refreshToken);
        
        _logger.LogInformation("Connexion réussie pour : {Email}", request.Email);
        return new TokenResponse 
        { 
            Token = accessToken, 
            RefreshToken = refreshToken.Token,
            Expiration = DateTime.UtcNow.AddMinutes(60),
            User = new UserInfoResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Roles = new List<string> { user.Role }.Concat(permissions).Distinct().ToList(),
                TenantId = user.TenantId,
                CanSeeAllTenantData = user.CanSeeAllTenantData
            }
        };
    }

    /// <inheritdoc />
    public async Task<TokenResponse> RefreshTokenAsync(string token, string refreshToken)
    {
        _logger.LogInformation("Tentative de rafraîchissement du token");

        // 1. Validation du Refresh Token en base
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (storedToken == null)
        {
            _logger.LogWarning("Refresh token invalide");
            throw new SecurityTokenException("Refresh token invalide");
        }

        if (storedToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh token expiré (ID: {Id})", storedToken.Id);
            throw new SecurityTokenException("Refresh token expiré");
        }

        if (storedToken.IsRevoked)
        {
            _logger.LogCritical("Tentative d'utilisation d'un token révoqué ! (ID: {Id})", storedToken.Id);
            throw new SecurityTokenException("Refresh token révoqué");
        }

        // 2. Récupération de l'utilisateur (via UserService)
        var user = await _userService.GetUserByIdAsync(storedToken.UserId);
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("Utilisateur introuvable ou inactif lors du refresh (UserID: {UserId})", storedToken.UserId);
            throw new UnauthorizedAccessException("Utilisateur invalide ou inactif");
        }

        // 3. Rotation des Tokens
        _logger.LogDebug("Rotation des tokens pour l'utilisateur : {Email}", user.Email);
        
        IEnumerable<string> permissions;
        if (user.Role == "SuperAdmin")
        {
            _logger.LogInformation("Utilisateur SuperAdmin détecté lors du refresh. Attribution de TOUTES les permissions.");
            permissions = await _permissionRepository.GetAllPermissionSystemNamesAsync();
        }
        else
        {
            permissions = await _permissionRepository.GetPermissionsByUserIdAsync(user.Id);
        }
        
        var newAccessToken = _tokenService.GenerateToken(user, user.TenantId.ToString(), permissions);
        var newRefreshToken = _tokenService.GenerateRefreshToken(user.Id);

        // 4. Révocation de l'ancien token
        await _refreshTokenRepository.RevokeAsync(refreshToken, newRefreshToken.Token);
        
        // 5. Sauvegarde du nouveau token
        await _refreshTokenRepository.CreateAsync(newRefreshToken);

        _logger.LogInformation("Tokens rafraîchis avec succès pour : {Email}", user.Email);
        return new TokenResponse 
        { 
            Token = newAccessToken, 
            RefreshToken = newRefreshToken.Token,
            Expiration = DateTime.UtcNow.AddMinutes(60),
            User = new UserInfoResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Roles = new List<string> { user.Role }.Concat(permissions).Distinct().ToList(),
                TenantId = user.TenantId,
                CanSeeAllTenantData = user.CanSeeAllTenantData
            }
        };
    }
}
