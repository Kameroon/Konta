using Konta.Finance.Models;

namespace Konta.Finance.Services.Interfaces;

public interface IAccountingService
{
    /// <summary>
    /// Valide et enregistre une écriture comptable.
    /// </summary>
    Task<Guid> PostEntryAsync(JournalEntry entry);
    
    /// <summary>
    /// Calcule le solde d'un compte sur une période donnée.
    /// </summary>
    Task<decimal> GetAccountBalanceAsync(Guid accountId, DateTime? endDate = null);
    
    /// <summary>
    /// Initialise le plan comptable par défaut pour un nouveau tenant.
    /// </summary>
    Task InitializeDefaultAccountsAsync(Guid tenantId);
}
