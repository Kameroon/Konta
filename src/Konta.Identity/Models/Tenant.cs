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
    /// Identifiant unique (slug ou domaine).
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// Secteur d'activité.
    /// </summary>
    public string? Industry { get; set; }

    /// <summary>
    /// Adresse physique.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Numéro SIRET de l'entreprise.
    /// </summary>
    public string? Siret { get; set; }

    /// <summary>
    /// Plan d'abonnement actuel (version simplifiée).
    /// </summary>
    public string Plan { get; set; } = "Free";
}
