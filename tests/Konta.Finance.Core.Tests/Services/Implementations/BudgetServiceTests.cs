using Konta.Finance.Core.Data.Repositories.Interfaces;
using Konta.Finance.Core.Models;
using Konta.Finance.Core.Services.Implementations;
using Microsoft.Extensions.Logging;
using NSubstitute;
using FluentAssertions;
using Xunit;

namespace Konta.Finance.Core.Tests.Services.Implementations;

public class BudgetServiceTests
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IFinanceAlertRepository _alertRepository;
    private readonly ILogger<BudgetService> _logger;
    private readonly BudgetService _sut; // System Under Test

    public BudgetServiceTests()
    {
        _budgetRepository = Substitute.For<IBudgetRepository>();
        _alertRepository = Substitute.For<IFinanceAlertRepository>();
        _logger = Substitute.For<ILogger<BudgetService>>();
        
        _sut = new BudgetService(_budgetRepository, _alertRepository, _logger);
    }

    [Fact]
    public async Task TrackSpendingAsync_WhenUnderThreshold_ShouldNotCreateAlert()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var category = "Technologie";
        var amount = 100m;
        var budget = new Budget
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Category = category,
            AllocatedAmount = 1000m,
            SpentAmount = 500m,
            AlertThresholdPercentage = 80m
        };

        _budgetRepository.GetCurrentBudgetsAsync(tenantId, Arg.Any<DateTime>())
            .Returns(new List<Budget> { budget });

        // Act
        await _sut.TrackSpendingAsync(tenantId, category, amount);

        // Assert
        await _budgetRepository.Received(1).UpdateSpentAmountAsync(budget.Id, amount);
        await _alertRepository.DidNotReceiveWithAnyArgs().CreateAsync(Arg.Any<FinanceAlert>());
    }

    [Fact]
    public async Task TrackSpendingAsync_WhenExceedingThreshold_ShouldCreateWarningAlert()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var category = "Technologie";
        var amount = 400m; // Nouveau total = 500 + 400 = 900 (90%)
        var budget = new Budget
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Category = category,
            AllocatedAmount = 1000m,
            SpentAmount = 500m,
            AlertThresholdPercentage = 80m
        };

        _budgetRepository.GetCurrentBudgetsAsync(tenantId, Arg.Any<DateTime>())
            .Returns(new List<Budget> { budget });

        // Act
        await _sut.TrackSpendingAsync(tenantId, category, amount);

        // Assert
        await _alertRepository.Received(1).CreateAsync(Arg.Is<FinanceAlert>(a => 
            a.Severity == "Warning" && 
            a.TenantId == tenantId &&
            a.Message.Contains("90,0%")
        ));
    }

    [Fact]
    public async Task TrackSpendingAsync_WhenExceedingTotalAllocation_ShouldCreateCriticalAlert()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var category = "Technologie";
        var amount = 600m; // Nouveau total = 500 + 600 = 1100 (110%)
        var budget = new Budget
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Category = category,
            AllocatedAmount = 1000m,
            SpentAmount = 500m,
            AlertThresholdPercentage = 80m
        };

        _budgetRepository.GetCurrentBudgetsAsync(tenantId, Arg.Any<DateTime>())
            .Returns(new List<Budget> { budget });

        // Act
        await _sut.TrackSpendingAsync(tenantId, category, amount);

        // Assert
        await _alertRepository.Received(1).CreateAsync(Arg.Is<FinanceAlert>(a => 
            a.Severity == "Critical" && 
            a.Message.Contains("totalement épuisé")
        ));
    }
}
