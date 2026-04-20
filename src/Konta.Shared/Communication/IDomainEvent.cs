using MediatR;

namespace Konta.Shared.Communication;

/// <summary>
/// Interface de base pour tous les événements de domaine internes.
/// </summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}

/// <summary>
/// Implémentation de base d'un événement de domaine.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
