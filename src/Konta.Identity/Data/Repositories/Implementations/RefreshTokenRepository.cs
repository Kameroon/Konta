using Dapper;
using Konta.Identity.Data.Repositories.Interfaces;
using Konta.Identity.Models;
using Microsoft.Extensions.Logging;
using Konta.Shared.Data;
using Konta.Shared.Data.Repositories;

namespace Konta.Identity.Data.Repositories.Implementations;

/// <summary>
/// Repository pour la gestion des Refresh Tokens.
/// </summary>
public class RefreshTokenRepository : BaseRepository<RefreshTokenRepository>, IRefreshTokenRepository
{
    public RefreshTokenRepository(IDbConnectionFactory connectionFactory, ILogger<RefreshTokenRepository> logger)
        : base(connectionFactory, logger)
    {
    }

    /// <inheritdoc />
    public async Task CreateAsync(RefreshToken token)
    {
        _logger.LogDebug("Sauvegarde du Refresh Token pour l'utilisateur ID : {UserId}", token.UserId);
        const string sql = @"
            INSERT INTO identity.RefreshTokens (Id, UserId, Token, ExpiresAt, IsRevoked, CreatedAt) 
            VALUES (@Id, @UserId, @Token, @ExpiresAt, @IsRevoked, @CreatedAt)";
        using var connection = CreateConnection(sql, token);
        await connection.ExecuteAsync(sql, token);
    }

    /// <inheritdoc />
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        const string sql = "SELECT * FROM identity.RefreshTokens WHERE Token = @Token";
        using var connection = CreateConnection(sql, new { Token = token });
        return await connection.QuerySingleOrDefaultAsync<RefreshToken>(sql, new { Token = token });
    }

    /// <inheritdoc />
    public async Task RevokeAsync(string token, string replacedByToken = null)
    {
        _logger.LogDebug("Révocation du token : {Token}...", token[..10]);
        const string sql = @"
            UPDATE identity.RefreshTokens 
            SET IsRevoked = TRUE, ReplacedByToken = @ReplacedByToken, UpdatedAt = NOW() 
            WHERE Token = @Token";
        using var connection = CreateConnection(sql, new { Token = token, ReplacedByToken = replacedByToken });
        await connection.ExecuteAsync(sql, new { Token = token, ReplacedByToken = replacedByToken });
    }
}
