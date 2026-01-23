using Konta.Identity.Models;

namespace Konta.Identity.Data.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(User user);
    Task<IEnumerable<User>> GetAllByTenantIdAsync(Guid tenantId);
    Task<IEnumerable<User>> GetAllAsync();
    Task<bool> UpdateAsync(User user);
    Task<bool> DeleteAsync(Guid id);
}
