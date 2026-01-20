namespace Konta.Shared.Models;

/// <summary>
/// Classe de base pour toutes les entités du système, incluant les champs d'audit.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Identifiant unique global de l'entité.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Date et heure de création de l'entité.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Date et heure de la dernière modification de l'entité.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Indique si l'entité est active (utilisé pour la suppression logique).
    /// </summary>
    public bool IsActive { get; set; } = true;
}
