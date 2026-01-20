using Dapper;
using Konta.Finance.Core.Data.Repositories.Interfaces;
using Konta.Finance.Core.Models;
using Konta.Shared.Data;
using Konta.Shared.Data.Repositories;

namespace Konta.Finance.Core.Data.Repositories.Implementations;

/// <summary>
/// Gestion de la persistance des budgets.
/// </summary>
public class BudgetRepository : BaseRepository<BudgetRepository>, IBudgetRepository
{
    public BudgetRepository(IDbConnectionFactory connectionFactory, ILogger<BudgetRepository> logger) 
        : base(connectionFactory, logger)
    {
    }

    /// <summary>
    /// Récupère un budget par son ID.
    /// </summary>
    public async Task<Budget?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT * FROM Budgets WHERE Id = @Id";
        
        _logger.LogDebug("Recherche du budget : {Id}", id);
        
        using var connection = CreateConnection(sql, new { Id = id });
        return await connection.QuerySingleOrDefaultAsync<Budget>(sql, new { Id = id });
    }

    /// <summary>
    /// Récupère la liste des budgets actifs pour une date donnée.
    /// </summary>
    public async Task<IEnumerable<Budget>> GetCurrentBudgetsAsync(Guid tenantId, DateTime date)
    {
        const string sql = @"
            SELECT * FROM Budgets 
            WHERE TenantId = @TenantId AND StartDate <= @Date AND EndDate >= @Date 
            ORDER BY Category ASC";
            
        _logger.LogDebug("Récupération des budgets actifs pour le tenant {TenantId} à la date {Date}", tenantId, date);
        
        using var connection = CreateConnection(sql, new { TenantId = tenantId, Date = date });
        return await connection.QueryAsync<Budget>(sql, new { TenantId = tenantId, Date = date });
    }

    /// <summary>
    /// Crée un nouveau budget.
    /// </summary>
    public async Task<Guid> CreateAsync(Budget budget)
    {
        const string sql = @"
            INSERT INTO Budgets (Id, TenantId, Category, AllocatedAmount, SpentAmount, StartDate, EndDate, AlertThresholdPercentage, CreatedAt)
            VALUES (@Id, @TenantId, @Category, @AllocatedAmount, @SpentAmount, @StartDate, @EndDate, @AlertThresholdPercentage, @CreatedAt)
            RETURNING Id";
            
        _logger.LogInformation("Initialisation d'un nouveau budget : {Category} (Montant: {Amount})", budget.Category, budget.AllocatedAmount);
        
        using var connection = CreateConnection(sql, budget);
        return await connection.ExecuteScalarAsync<Guid>(sql, budget);
    }

    /// <summary>
    /// Incrémente le montant consommé d'un budget.
    /// </summary>
    public async Task<bool> UpdateSpentAmountAsync(Guid id, decimal amount)
    {
        const string sql = "UPDATE Budgets SET SpentAmount = SpentAmount + @Amount, UpdatedAt = @UpdatedAt WHERE Id = @Id";
        
        _logger.LogInformation("Mise à jour de la consommation budgétaire pour {Id} : +{Amount}", id, amount);
        
        var parameters = new { Id = id, Amount = amount, UpdatedAt = DateTime.UtcNow };
        using var connection = CreateConnection(sql, parameters);
        var rows = await connection.ExecuteAsync(sql, parameters);
        return rows > 0;
    }
}
