namespace Konta.Identity.DTOs;

/// <summary>
/// Requête pour promouvoir un utilisateur au rôle SuperAdmin.
/// </summary>
public record PromoteRequest
{
    /// <summary>
    /// Email de l'utilisateur à promouvoir.
    /// </summary>
    public string Email { get; init; } = string.Empty;
}
