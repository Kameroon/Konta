namespace Konta.Identity.DTOs;

/// <summary>
/// Informations de base d'un utilisateur transmises après authentification.
/// </summary>
public record UserInfoResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = new();
    public Guid TenantId { get; init; }
}
