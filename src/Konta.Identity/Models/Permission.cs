using Konta.Shared.Models;

namespace Konta.Identity.Models;

/// <summary>
/// Représente une action ou un droit spécifique dans le système.
/// </summary>
public class Permission : BaseEntity
{
    /// <summary>
    /// Nom technique unique de la permission (ex: 'user.create').
    /// </summary>
    public string SystemName { get; set; } = string.Empty;

    /// <summary>
    /// Nom lisible par l'utilisateur.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description de ce que cette permission autorise.
    /// </summary>
    public string? Description { get; set; }
}
