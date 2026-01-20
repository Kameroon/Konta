using Dapper;
using Konta.Billing.Data.Repositories.Interfaces;
using Konta.Billing.Models;
using Konta.Shared.Data;
using Konta.Shared.Data.Repositories;

namespace Konta.Billing.Data.Repositories.Implementations;

public class BillingInvoiceRepository : BaseRepository<BillingInvoiceRepository>, IBillingInvoiceRepository
{
    public BillingInvoiceRepository(IDbConnectionFactory connectionFactory, ILogger<BillingInvoiceRepository> logger) 
        : base(connectionFactory, logger)
    {
    }

    public async Task<IEnumerable<BillingInvoice>> GetByTenantIdAsync(Guid tenantId)
    {
        const string sql = "SELECT * FROM billing.BillingInvoices WHERE TenantId = @TenantId ORDER BY CreatedAt DESC";
        using var connection = CreateConnection(sql, new { TenantId = tenantId });
        return await connection.QueryAsync<BillingInvoice>(sql, new { TenantId = tenantId });
    }

    public async Task<BillingInvoice?> GetByStripeIdAsync(string stripeInvoiceId)
    {
        const string sql = "SELECT * FROM billing.BillingInvoices WHERE StripeInvoiceId = @StripeInvoiceId";
        using var connection = CreateConnection(sql, new { StripeInvoiceId = stripeInvoiceId });
        return await connection.QueryFirstOrDefaultAsync<BillingInvoice>(sql, new { StripeInvoiceId = stripeInvoiceId });
    }

    public async Task<Guid> CreateAsync(BillingInvoice invoice)
    {
        const string sql = @"
            INSERT INTO billing.BillingInvoices (Id, TenantId, StripeInvoiceId, InvoiceNumber, Amount, Currency, Status, PdfUrl, CreatedAt, UpdatedAt, IsActive)
            VALUES (@Id, @TenantId, @StripeInvoiceId, @InvoiceNumber, @Amount, @Currency, @Status, @PdfUrl, @CreatedAt, @UpdatedAt, @IsActive)
            RETURNING Id";
        
        using var connection = CreateConnection(sql, invoice);
        return await connection.ExecuteScalarAsync<Guid>(sql, invoice);
    }

    public async Task UpdateStatusAsync(string stripeInvoiceId, string status, DateTime? paidAt = null)
    {
        const string sql = @"
            UPDATE billing.BillingInvoices 
            SET Status = @Status, PaidAt = @PaidAt, UpdatedAt = @UpdatedAt 
            WHERE StripeInvoiceId = @StripeInvoiceId";
        
        var parameters = new 
        { 
            StripeInvoiceId = stripeInvoiceId, 
            Status = status, 
            PaidAt = paidAt, 
            UpdatedAt = DateTime.UtcNow 
        };
        
        using var connection = CreateConnection(sql, parameters);
        await connection.ExecuteAsync(sql, parameters);
    }
}
