using Konta.Shared.Models;

namespace Konta.Identity.Models;

/// <summary>
/// Représente une entreprise (Tenant) dans le module d'identité.
/// </summary>
public class Tenant : BaseEntity
{
    /// <summary>
    /// Nom de l'entreprise.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Plan d'abonnement actuel (version simplifiée).
    /// </summary>
    public string Plan { get; set; } = "Free";
}
