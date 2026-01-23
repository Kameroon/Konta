using Konta.Identity.DTOs;

namespace Konta.Identity.Services.Interfaces;

/// <summary>
/// Interface for Tenant management services.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Registers a new tenant along with an admin user.
    /// </summary>
    /// <param name="request">The registration request containing tenant and user details.</param>
    /// <returns>The ID of the created user (admin of the tenant).</returns>
    Task<Guid> RegisterTenantAsync(RegisterRequest request);

    /// <summary>
    /// Retrieves a tenant by its ID.
    /// </summary>
    Task<TenantResponse?> GetTenantByIdAsync(Guid id);

    /// <summary>
    /// Retrieves all tenants.
    /// </summary>
    Task<IEnumerable<TenantResponse>> GetAllTenantsAsync();

    /// <summary>
    /// Updates an existing tenant.
    /// </summary>
    Task<bool> UpdateTenantAsync(Guid id, UpdateTenantRequest request);

    /// <summary>
    /// Updates only the subscription plan of a tenant.
    /// </summary>
    Task<bool> UpdatePlanAsync(Guid id, string plan);

    /// <summary>
    /// Deletes a tenant.
    /// </summary>
    Task<bool> DeleteTenantAsync(Guid id);
}
