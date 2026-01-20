using Konta.Shared.Models;

namespace Konta.Finance.Core.Models;

/// <summary>
/// Représente un compte de trésorerie (Banque, Caisse).
/// </summary>
public class TreasuryAccount : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AccountNumber { get; set; } // IBAN ou Identifiant
    public decimal CurrentBalance { get; set; }
    public string Currency { get; set; } = "EUR";
}
