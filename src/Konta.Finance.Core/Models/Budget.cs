using Konta.Shared.Models;

namespace Konta.Finance.Core.Models;

/// <summary>
/// Représente une enveloppe budgétaire.
/// </summary>
public class Budget : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Category { get; set; } = string.Empty; // ex: "Marketing", "Salaires"
    public decimal AllocatedAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal AlertThresholdPercentage { get; set; } = 90; // Alerte à 90%
}
