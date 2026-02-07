using Konta.Identity.Models;

namespace Konta.Identity.Data.Repositories.Interfaces;

public interface INavigationRepository
{
    Task<IEnumerable<NavigationItem>> GetAllAsync();
    Task<NavigationItem?> GetByIdAsync(Guid id);
    Task<bool> UpdateAsync(NavigationItem item);
}
