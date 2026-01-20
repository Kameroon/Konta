using Dapper;
using Konta.Billing.Data.Repositories.Interfaces;
using Konta.Billing.Models;
using Konta.Shared.Data;
using Konta.Shared.Data.Repositories;

namespace Konta.Billing.Data.Repositories.Implementations;

public class StripeCustomerRepository : BaseRepository<StripeCustomerRepository>, IStripeCustomerRepository
{
    public StripeCustomerRepository(IDbConnectionFactory connectionFactory, ILogger<StripeCustomerRepository> logger) 
        : base(connectionFactory, logger)
    {
    }

    public async Task<StripeCustomer?> GetByTenantIdAsync(Guid tenantId)
    {
        const string sql = "SELECT * FROM StripeCustomers WHERE TenantId = @TenantId AND IsActive = true";
        using var connection = CreateConnection(sql, new { TenantId = tenantId });
        return await connection.QueryFirstOrDefaultAsync<StripeCustomer>(sql, new { TenantId = tenantId });
    }

    public async Task<StripeCustomer?> GetByStripeIdAsync(string stripeCustomerId)
    {
        const string sql = "SELECT * FROM StripeCustomers WHERE StripeCustomerId = @StripeCustomerId AND IsActive = true";
        using var connection = CreateConnection(sql, new { StripeCustomerId = stripeCustomerId });
        return await connection.QueryFirstOrDefaultAsync<StripeCustomer>(sql, new { StripeCustomerId = stripeCustomerId });
    }

    public async Task<Guid> CreateAsync(StripeCustomer customer)
    {
        const string sql = @"
            INSERT INTO StripeCustomers (Id, TenantId, StripeCustomerId, Email, CreatedAt, UpdatedAt, IsActive)
            VALUES (@Id, @TenantId, @StripeCustomerId, @Email, @CreatedAt, @UpdatedAt, @IsActive)
            RETURNING Id";
        
        using var connection = CreateConnection(sql, customer);
        return await connection.ExecuteScalarAsync<Guid>(sql, customer);
    }
}
