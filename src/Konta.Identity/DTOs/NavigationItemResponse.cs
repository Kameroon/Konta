namespace Konta.Identity.DTOs;

public record NavigationItemResponse(
    Guid Id,
    string Label,
    string Path,
    string? Icon,
    string? RequiredPermission,
    string? RequiredRole,
    int DisplayOrder,
    bool IsVisible
);
