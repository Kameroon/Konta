namespace Konta.Identity.DTOs;

public record UpdateNavigationItemRequest(
    string Label,
    string Path,
    string? Icon,
    string? RequiredPermission,
    string? RequiredRole,
    int DisplayOrder,
    bool IsVisible
);
