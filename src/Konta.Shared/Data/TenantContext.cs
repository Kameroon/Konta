namespace Konta.Shared.Data;

public interface ITenantContext
{
    Guid? TenantId { get; set; }
    Guid? UserId { get; set; }
    bool IsGlobalAdmin { get; set; }
    bool CanSeeAllTenantData { get; set; }
}

public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }
    public bool IsGlobalAdmin { get; set; }
    public bool CanSeeAllTenantData { get; set; }
}
