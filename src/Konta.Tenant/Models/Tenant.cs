using Konta.Shared.Models;

namespace Konta.Tenant.Models;

/// <summary>
/// Représente une entreprise cliente (Tenant) dans le système.
/// </summary>
public class Tenant : BaseEntity
{
    /// <summary>
    /// Nom officiel de l'entreprise.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Identifiant unique (slug ou domaine) utilisé pour l'URL ou l'identification.
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// Secteur d'activité de l'entreprise.
    /// </summary>
    public string? Industry { get; set; }

    /// <summary>
    /// Adresse physique du siège social.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Identifiant fiscal unique de l'entreprise.
    /// </summary>
    public string? TaxId { get; set; }

    /// <summary>
    /// Plan d'abonnement actuel.
    /// </summary>
    public string Plan { get; set; } = "Free";
}
