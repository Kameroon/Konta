namespace Konta.Identity.DTOs;

/// <summary>
/// Représentation d'une permission pour les réponses API.
/// </summary>
public record PermissionResponse
{
    public Guid Id { get; init; }
    public string SystemName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}
