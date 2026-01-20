using Konta.Shared.Models;

namespace Konta.Finance.Core.Models;

public enum TierType
{
    Client = 1,
    Supplier = 2
}

/// <summary>
/// Représente un tiers (Client ou Fournisseur).
/// </summary>
public class Tier : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public TierType Type { get; set; }
    public string? Email { get; set; }
    public string? TaxId { get; set; }
    public string? Address { get; set; }
}
