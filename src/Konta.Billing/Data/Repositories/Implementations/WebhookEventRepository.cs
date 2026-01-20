using Dapper;
using Konta.Billing.Data.Repositories.Interfaces;
using Konta.Billing.Models;
using Konta.Shared.Data;
using Konta.Shared.Data.Repositories;

namespace Konta.Billing.Data.Repositories.Implementations;

public class WebhookEventRepository : BaseRepository<WebhookEventRepository>, IWebhookEventRepository
{
    public WebhookEventRepository(IDbConnectionFactory connectionFactory, ILogger<WebhookEventRepository> logger) 
        : base(connectionFactory, logger)
    {
    }

    public async Task<bool> ExistsAsync(string stripeEventId)
    {
        const string sql = "SELECT COUNT(1) FROM WebhookEvents WHERE StripeEventId = @StripeEventId";
        using var connection = CreateConnection(sql, new { StripeEventId = stripeEventId });
        return await connection.ExecuteScalarAsync<int>(sql, new { StripeEventId = stripeEventId }) > 0;
    }

    public async Task AddAsync(WebhookEvent @event)
    {
        const string sql = @"
            INSERT INTO WebhookEvents (Id, StripeEventId, EventType, ReceivedAt, ProcessedSuccessfully, CreatedAt, UpdatedAt, IsActive)
            VALUES (@Id, @StripeEventId, @EventType, @ReceivedAt, @ProcessedSuccessfully, @CreatedAt, @UpdatedAt, @IsActive)";
            
        using var connection = CreateConnection(sql, @event);
        await connection.ExecuteAsync(sql, @event);
    }
}
