using Konta.Identity.Data.Repositories.Interfaces;
using Konta.Identity.DTOs;
using Konta.Identity.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Konta.Identity.Services.Implementations;

public class NavigationService : INavigationService
{
    private readonly INavigationRepository _navigationRepository;
    private readonly ILogger<NavigationService> _logger;

    public NavigationService(INavigationRepository navigationRepository, ILogger<NavigationService> logger)
    {
        _navigationRepository = navigationRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<NavigationItemResponse>> GetAllAsync()
    {
        var items = await _navigationRepository.GetAllAsync();
        return items.Select(i => new NavigationItemResponse(
            i.Id,
            i.Label,
            i.Path,
            i.Icon,
            i.RequiredPermission,
            i.RequiredRole,
            i.DisplayOrder,
            i.IsVisible
        ));
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateNavigationItemRequest request)
    {
        var existing = await _navigationRepository.GetByIdAsync(id);
        if (existing == null) return false;

        existing.Label = request.Label;
        existing.Path = request.Path;
        existing.Icon = request.Icon;
        existing.RequiredPermission = request.RequiredPermission;
        existing.RequiredRole = request.RequiredRole;
        existing.DisplayOrder = request.DisplayOrder;
        existing.IsVisible = request.IsVisible;

        return await _navigationRepository.UpdateAsync(existing);
    }
}
