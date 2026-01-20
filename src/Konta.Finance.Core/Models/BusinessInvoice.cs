using Konta.Shared.Models;

namespace Konta.Finance.Core.Models;

public enum InvoiceStatus
{
    Draft = 0,
    Validated = 1,
    Paid = 2,
    Canceled = 3
}

/// <summary>
/// Représente une facture opérationnelle (Vente ou Achat).
/// </summary>
public class BusinessInvoice : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid TierId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public decimal AmountHt { get; set; }
    public decimal AmountTtc { get; set; }
    public decimal VatAmount { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public bool IsPurchase { get; set; } // True = Fournisseur, False = Client
}
