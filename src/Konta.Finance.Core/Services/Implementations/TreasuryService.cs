using Konta.Finance.Core.Data.Repositories.Interfaces;
using Konta.Finance.Core.Models;
using Konta.Finance.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Konta.Finance.Core.Services.Implementations;

/// <summary>
/// Service gérant les flux de trésorerie et la surveillance des comptes bancaires.
/// </summary>
public class TreasuryService : ITreasuryService
{
    private readonly ITreasuryRepository _treasuryRepository;
    private readonly IFinanceAlertRepository _alertRepository;
    private readonly ILogger<TreasuryService> _logger;

    public TreasuryService(
        ITreasuryRepository treasuryRepository,
        IFinanceAlertRepository alertRepository,
        ILogger<TreasuryService> logger)
    {
        _treasuryRepository = treasuryRepository;
        _alertRepository = alertRepository;
        _logger = logger;
    }

    /// <summary>
    /// Enregistre une entrée ou sortie d'argent sur un compte spécifique.
    /// </summary>
    public async Task RegisterMovementAsync(Guid accountId, decimal amount, bool isIncome)
    {
        _logger.LogInformation("Traitement mouvement trésorerie sur compte {AccountId} : {Amount}€ ({Type})", 
            accountId, amount, isIncome ? "Entrée" : "Sortie");

        // Recherche du compte concerné
        var account = await _treasuryRepository.GetByIdAsync(accountId);
        if (account == null)
        {
            _logger.LogError("❌ ÉCHEC : Compte de trésorerie {AccountId} introuvable pour le mouvement.", accountId);
            throw new KeyNotFoundException("Compte de trésorerie introuvable.");
        }

        // Calcul du nouveau solde
        _logger.LogDebug("Ancien solde pour {AccountName} : {Balance}", account.Name, account.CurrentBalance);
        var newBalance = isIncome ? account.CurrentBalance + amount : account.CurrentBalance - amount;
        
        // Persistance du nouveau solde
        await _treasuryRepository.UpdateBalanceAsync(accountId, newBalance);

        _logger.LogInformation("✅ SUCCÈS : Nouveau solde de {AccountName} est {Balance} {Currency}", 
            account.Name, newBalance, account.Currency);

        // Analyse de risque : Déclenchement d'alerte si le solde devient négatif
        if (newBalance < 0)
        {
            _logger.LogWarning("🚨 ALERTE TRÉSORERIE : Solde négatif détecté sur {AccountName}", account.Name);
            await _alertRepository.CreateAsync(new FinanceAlert
            {
                TenantId = account.TenantId,
                Title = "Trésorerie Critique",
                Message = $"Le compte '{account.Name}' est passé en découvert ({newBalance:F2} {account.Currency}). Action immédiate recommandée.",
                Severity = "Critical",
                RelatedEntityId = account.Id,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
