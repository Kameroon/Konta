namespace Konta.Finance.Core.Services.Interfaces;

public interface ITreasuryService
{
    /// <summary>
    /// Enregistre un mouvement de fonds sur un compte.
    /// </summary>
    Task RegisterMovementAsync(Guid accountId, decimal amount, bool isIncome);
}
