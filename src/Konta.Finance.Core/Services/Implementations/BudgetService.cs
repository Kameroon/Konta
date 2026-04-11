using Konta.Finance.Core.Data.Repositories.Interfaces;
using Konta.Finance.Core.Models;
using Konta.Finance.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Konta.Finance.Core.Services.Implementations;

/// <summary>
/// Service gérant les enveloppes budgétaires et les alertes automatiques.
/// </summary>
public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IFinanceAlertRepository _alertRepository;
    private readonly ILogger<BudgetService> _logger;

    public BudgetService(
        IBudgetRepository budgetRepository, 
        IFinanceAlertRepository alertRepository,
        ILogger<BudgetService> logger)
    {
        _budgetRepository = budgetRepository;
        _alertRepository = alertRepository;
        _logger = logger;
    }

    /// <summary>
    /// Enregistre une nouvelle dépense sur un budget et déclenche des alertes si nécessaire.
    /// </summary>
    public async Task TrackSpendingAsync(Guid tenantId, string category, decimal amount, Guid? relatedEntityId = null)
    {
        _logger.LogInformation("Enregistrement dépense sur budget : {Amount}€ sur catégorie '{Category}' pour Tenant {TenantId}", amount, category, tenantId);

        // Récupérer les budgets actifs pour la catégorie demandée
        var currentBudgets = await _budgetRepository.GetCurrentBudgetsAsync(tenantId, DateTime.UtcNow);
        var budget = currentBudgets.FirstOrDefault(b => b.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

        if (budget == null)
        {
            _logger.LogWarning("⚠️ Aucun budget actif trouvé pour la catégorie : {Category}. La dépense ne sera pas suivie budgétairement.", category);
            return;
        }

        // Mise à jour de la consommation en base
        _logger.LogDebug("Mise à jour de la consommation du budget {Id}", budget.Id);
        await _budgetRepository.UpdateSpentAmountAsync(budget.Id, amount);
        
        // Calcul du nouveau ratio de consommation pour l'alerte
        var newSpent = budget.SpentAmount + amount;
        var ratio = (newSpent / budget.AllocatedAmount) * 100;

        _logger.LogDebug("Nouveau ratio budgétaire pour {Category} : {Ratio:F1}%", category, ratio);

        // Déclenchement de l'alerte si le seuil configuré est atteint ou dépassé
        if (ratio >= budget.AlertThresholdPercentage)
        {
            await CreateBudgetAlertAsync(budget, ratio, relatedEntityId);
        }
    }

    /// <summary>
    /// Effectue un contrôle de santé sur tous les budgets d'une entreprise.
    /// </summary>
    public async Task CheckBudgetAlertsAsync(Guid tenantId)
    {
        _logger.LogInformation("Lancement du contrôle global des budgets pour le tenant {TenantId}", tenantId);
        
        var currentBudgets = await _budgetRepository.GetCurrentBudgetsAsync(tenantId, DateTime.UtcNow);
        foreach (var budget in currentBudgets)
        {
            var ratio = (budget.SpentAmount / budget.AllocatedAmount) * 100;
            if (ratio >= budget.AlertThresholdPercentage)
            {
                _logger.LogInformation("Vérification proactive : seuil dépassé pour {Category} ({Ratio:F1}%)", budget.Category, ratio);
                await CreateBudgetAlertAsync(budget, ratio);
            }
        }
    }

    /// <summary>
    /// Logique interne de création d'alerte financière.
    /// </summary>
    private async Task CreateBudgetAlertAsync(Budget budget, decimal ratio, Guid? relatedEntityId = null)
    {
        // Détermine la gravité en fonction du dépassement
        var severity = ratio >= 100 ? "Critical" : "Warning";
        var message = ratio >= 100 
            ? $"🚨 ALERTE CRITIQUE : Le budget '{budget.Category}' est totalement épuisé ({ratio:F1}%)." 
            : $"⚠️ ATTENTION : Le budget '{budget.Category}' a consommé {ratio:F1}% de son allocation.";

        var alert = new FinanceAlert
        {
            TenantId = budget.TenantId,
            Title = $"Alerte Budget : {budget.Category}",
            Message = message,
            Severity = severity,
            RelatedEntityId = relatedEntityId ?? budget.Id,
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogWarning("DÉCLENCHEMENT ALERTE COMPTABLE : {Message}", message);
        await _alertRepository.CreateAsync(alert);
    }
}
