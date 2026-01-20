using Konta.Shared.Models;

namespace Konta.Finance.Models;

/// <summary>
/// Ligne de détail d'une écriture comptable (Débit ou Crédit).
/// </summary>
public class EntryLine : BaseEntity
{
    public Guid EntryId { get; set; }
    public Guid AccountId { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}
