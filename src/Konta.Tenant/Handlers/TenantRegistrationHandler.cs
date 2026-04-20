using Konta.Shared.Events.Identity;
using Konta.Tenant.Services.Interfaces;
using Konta.Tenant.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Konta.Tenant.Handlers;

/// <summary>
/// Handler MediatR qui réagit à la demande de création de tenant provenant du module Identity.
/// </summary>
public class TenantRegistrationHandler : INotificationHandler<TenantRegistrationRequestedEvent>
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<TenantRegistrationHandler> _logger;

    public TenantRegistrationHandler(ITenantService tenantService, ILogger<TenantRegistrationHandler> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    public async Task Handle(TenantRegistrationRequestedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Réception d'une demande d'enregistrement MediatR pour le tenant : {Name}", notification.Name);

        var request = new CreateTenantRequest
        {
            Name = notification.Name,
            Identifier = notification.Identifier,
            Industry = notification.Industry,
            Address = notification.Address,
            Siret = notification.Siret
        };

        await _tenantService.CreateTenantAsync(request);
    }
}
