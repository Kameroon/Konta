using Konta.Shared.Models;

namespace Konta.Finance.Core.Models;

/// <summary>
/// Représente une alerte financière triggered.
/// </summary>
public class FinanceAlert : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "Warning"; // Warning, Critical
    public bool IsRead { get; set; }
    public Guid? RelatedEntityId { get; set; } // ID du budget ou de la facture concernée
}
