using Konta.Identity.Models;

namespace Konta.Identity.Data.Repositories.Interfaces;

public interface IRefreshTokenRepository
{
    Task CreateAsync(RefreshToken token);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task RevokeAsync(string token, string replacedByToken = null);
}
