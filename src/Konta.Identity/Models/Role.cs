using Konta.Shared.Models;

namespace Konta.Identity.Models;

/// <summary>
/// Représente un rôle utilisateur au sein d'un tenant.
/// </summary>
public class Role : BaseEntity
{
    /// <summary>
    /// Identifiant du tenant associé au rôle.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Nom du rôle (ex: 'Admin', 'User').
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description optionnelle des responsabilités du rôle.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indique si ce rôle est attribué par défaut aux nouveaux utilisateurs.
    /// </summary>
    public bool IsDefault { get; set; }
}
