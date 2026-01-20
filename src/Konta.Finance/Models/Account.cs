using Konta.Shared.Models;

namespace Konta.Finance.Models;

/// <summary>
/// Type de compte comptable selon le Plan Comptable Général.
/// </summary>
public enum AccountType
{
    Asset = 1,      // Actif
    Liability = 2,  // Passif
    Equity = 3,     // Capitaux Propres
    Revenue = 4,    // Revenus / Produits
    Expense = 5     // Dépenses / Charges
}

/// <summary>
/// Représente un compte du Plan Comptable.
/// </summary>
public class Account : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty; // ex: "411000"
    public string Name { get; set; } = string.Empty; // ex: "Clients"
    public AccountType Type { get; set; }
    public Guid? ParentId { get; set; }
    
    // Pour la gestion hiérarchique
    public string FullPath { get; set; } = string.Empty;
}
