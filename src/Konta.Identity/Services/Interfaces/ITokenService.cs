using Konta.Identity.Models;

namespace Konta.Identity.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user, string tenantId, IEnumerable<string> permissions);
    RefreshToken GenerateRefreshToken(Guid userId);
}
