using Konta.Shared.Models;

namespace Konta.Identity.Models;

/// <summary>
/// Représente un élément du menu de navigation.
/// </summary>
public class NavigationItem : BaseEntity
{
    /// <summary>
    /// Libellé affiché dans le menu.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Chemin de navigation (route).
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Classe d'icône (FontAwesome).
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Permission technique requise pour voir cet élément.
    /// </summary>
    public string? RequiredPermission { get; set; }

    /// <summary>
    /// Rôle spécifique requis pour voir cet élément.
    /// </summary>
    public string? RequiredRole { get; set; }

    /// <summary>
    /// Ordre d'affichage.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Indique si l'élément est visible.
    /// </summary>
    public bool IsVisible { get; set; } = true;
}
