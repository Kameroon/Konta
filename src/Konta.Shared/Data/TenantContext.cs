namespace Konta.Shared.Data;

public interface ITenantContext
{
    Guid? TenantId { get; set; }
}

public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; set; }
}
