namespace Konta.Identity.DTOs;

/// <summary>
/// Représentation d'un rôle pour les réponses API.
/// </summary>
public record RoleResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsDefault { get; init; }
}
