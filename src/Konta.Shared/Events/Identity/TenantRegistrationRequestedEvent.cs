using Konta.Shared.Communication;

namespace Konta.Shared.Events.Identity;

/// <summary>
/// Événement déclenché lors de la création d'un nouvel utilisateur/tenant.
/// </summary>
public record TenantRegistrationRequestedEvent(
    string Name,
    string Identifier,
    string Industry,
    string Address,
    string Siret,
    string AdminEmail,
    string AdminPassword) : DomainEvent;
