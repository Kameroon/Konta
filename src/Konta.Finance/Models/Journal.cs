using Konta.Shared.Models;

namespace Konta.Finance.Models;

/// <summary>
/// Type de journal comptable.
/// </summary>
public enum JournalType
{
    General = 1,
    Sales = 2,
    Purchase = 3,
    Cash = 4,
    Bank = 5
}

/// <summary>
/// Représente un journal d'écritures.
/// </summary>
public class Journal : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty; // ex: "VT"
    public string Name { get; set; } = string.Empty; // ex: "Ventes"
    public JournalType Type { get; set; }
}
