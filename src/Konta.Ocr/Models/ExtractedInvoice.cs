using System;
using Konta.Shared.Models;

namespace Konta.Ocr.Models;

/// <summary>
/// Données extraites d'une facture.
/// </summary>
public class ExtractedInvoice : BaseEntity
{
    public Guid JobId { get; set; }
    public string? VendorName { get; set; }
    public string? VendorSiret { get; set; }
    public string? CustomerSiret { get; set; }
    public string? VendorVatNumber { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateOnly? InvoiceDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public decimal? TotalAmountHt { get; set; }
    public decimal? TotalAmountTtc { get; set; }
    public decimal? VatAmount { get; set; }
    public string? Currency { get; set; }
    public int ConfidenceScore { get; set; }
    public string? RawJson { get; set; } // Pour garder une trace complète de l'IA
}
