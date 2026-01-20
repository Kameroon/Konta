using Konta.Shared.Models;

namespace Konta.Finance.Models;

/// <summary>
/// Entête d'une écriture comptable.
/// </summary>
public class JournalEntry : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid JournalId { get; set; }
    public DateTime EntryDate { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Propriété de navigation (non persistée directement en Dapper)
    public List<EntryLine> Lines { get; set; } = new();
}
