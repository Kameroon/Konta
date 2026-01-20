using Konta.Finance.Data.Repositories.Interfaces;
using Konta.Finance.Models;
using Konta.Finance.Services.Interfaces;

namespace Konta.Finance.Services.Implementations;

/// <summary>
/// Service gérant la logique métier comptable (validation, équilibrage).
/// </summary>
public class AccountingService : IAccountingService
{
    private readonly IJournalEntryRepository _entryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<AccountingService> _logger;

    public AccountingService(
        IJournalEntryRepository entryRepository, 
        IAccountRepository accountRepository,
        ILogger<AccountingService> logger)
    {
        _entryRepository = entryRepository;
        _accountRepository = accountRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Guid> PostEntryAsync(JournalEntry entry)
    {
        _logger.LogInformation("Tentative d'enregistrement d'une écriture : {Ref}", entry.Reference);

        // 1. Validation de l'équilibre Débit/Crédit
        decimal totalDebit = entry.Lines.Sum(l => l.Debit);
        decimal totalCredit = entry.Lines.Sum(l => l.Credit);

        if (totalDebit != totalCredit)
        {
            _logger.LogError("Écriture déséquilibrée : Débit={Debit}, Crédit={Credit}", totalDebit, totalCredit);
            throw new InvalidOperationException("L'écriture comptable doit être équilibrée (Total Débit = Total Crédit).");
        }

        if (totalDebit == 0)
        {
            throw new InvalidOperationException("L'écriture ne peut pas être vide ou nulle.");
        }

        // 2. Persistance
        var id = await _entryRepository.CreateAsync(entry);
        _logger.LogInformation("Écriture {Id} enregistrée avec succès.", id);
        
        return id;
    }

    /// <inheritdoc />
    public async Task<decimal> GetAccountBalanceAsync(Guid accountId, DateTime? endDate = null)
    {
        var end = endDate ?? DateTime.UtcNow;
        var lines = await _entryRepository.GetLinesByAccountIdAsync(accountId, DateTime.MinValue, end);
        
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null) throw new ArgumentException("Compte introuvable.");

        decimal balance = lines.Sum(l => l.Debit - l.Credit);

        // Pour les comptes de Passif et Revenus, on inverse souvent le signe pour l'affichage
        if (account.Type == AccountType.Liability || account.Type == AccountType.Equity || account.Type == AccountType.Revenue)
        {
            return balance * -1;
        }

        return balance;
    }

    /// <inheritdoc />
    public async Task InitializeDefaultAccountsAsync(Guid tenantId)
    {
        _logger.LogInformation("Initialisation du plan comptable PCG pour le tenant {Id}", tenantId);

        // Exemple simplifié de plan comptable (PCG France)
        var defaults = new List<Account>
        {
            new Account { TenantId = tenantId, Code = "101000", Name = "Capital", Type = AccountType.Equity },
            new Account { TenantId = tenantId, Code = "411000", Name = "Clients", Type = AccountType.Asset },
            new Account { TenantId = tenantId, Code = "401000", Name = "Fournisseurs", Type = AccountType.Liability },
            new Account { TenantId = tenantId, Code = "512000", Name = "Banque", Type = AccountType.Asset },
            new Account { TenantId = tenantId, Code = "607000", Name = "Achats de marchandises", Type = AccountType.Expense },
            new Account { TenantId = tenantId, Code = "707000", Name = "Ventes de marchandises", Type = AccountType.Revenue }
        };

        foreach (var acc in defaults)
        {
            await _accountRepository.CreateAsync(acc);
        }
    }
}
