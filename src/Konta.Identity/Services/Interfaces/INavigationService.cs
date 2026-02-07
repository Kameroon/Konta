using Konta.Identity.DTOs;

namespace Konta.Identity.Services.Interfaces;

public interface INavigationService
{
    Task<IEnumerable<NavigationItemResponse>> GetAllAsync();
    Task<bool> UpdateAsync(Guid id, UpdateNavigationItemRequest request);
}
