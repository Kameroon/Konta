using Konta.Shared.Models;

namespace Konta.Ocr.Models;

/// <summary>
/// Données extraites d'un RIB.
/// </summary>
public class ExtractedRib : BaseEntity
{
    public Guid JobId { get; set; }
    public string? BankName { get; set; }
    public string? Iban { get; set; }
    public string? Bic { get; set; }
    public string? AccountHolder { get; set; }
}
