namespace Konta.Shared.Data;

public interface ITenantContext
{
    Guid? TenantId { get; set; }
    bool IsGlobalAdmin { get; set; }
}

public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; set; }
    public bool IsGlobalAdmin { get; set; }
}
