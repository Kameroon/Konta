using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Konta.Identity.Models;
using Konta.Identity.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Konta.Identity.Services.Implementations;

/// <summary>
/// Service de génération des tokens JWT et Refresh Tokens.
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IConfiguration configuration, ILogger<TokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public string GenerateToken(User user, string tenantId, IEnumerable<string> permissions)
    {
        _logger.LogDebug("Génération du token JWT pour l'utilisateur {Email}", user.Email);

        var secret = _configuration["JwtSettings:Secret"] 
                     ?? throw new ArgumentNullException("JwtSettings:Secret");
        var issuer = _configuration["JwtSettings:Issuer"];
        var audience = _configuration["JwtSettings:Audience"];
        var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new("tenant_id", tenantId),
            new("role", user.Role), // Keep legacy role for simple checks
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        // Add permissions as claims
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permissions", permission));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    /// <inheritdoc />
    public RefreshToken GenerateRefreshToken(Guid userId)
    {
        _logger.LogDebug("Génération d'un nouveau Refresh Token pour l'utilisateur ID {UserId}", userId);

        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        var randomBytes = new byte[64];
        rng.GetBytes(randomBytes);
        
        return new RefreshToken
        {
            UserId = userId,
            Token = Convert.ToBase64String(randomBytes),
            ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 Days Refresh Window
            CreatedAt = DateTime.UtcNow
        };
    }
}
