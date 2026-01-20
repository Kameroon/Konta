using Dapper;
using Konta.Finance.Data.Repositories.Interfaces;
using Konta.Finance.Models;
using Konta.Shared.Data;
using Konta.Shared.Data.Repositories;

namespace Konta.Finance.Data.Repositories.Implementations;

public class JournalRepository : BaseRepository<JournalRepository>, IJournalRepository
{
    public JournalRepository(IDbConnectionFactory connectionFactory, ILogger<JournalRepository> logger) 
        : base(connectionFactory, logger)
    {
    }

    public async Task<Journal?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT * FROM finance.Journals WHERE Id = @Id";
        using var connection = CreateConnection(sql, new { Id = id });
        return await connection.QuerySingleOrDefaultAsync<Journal>(sql, new { Id = id });
    }

    public async Task<Journal?> GetByCodeAsync(Guid tenantId, string code)
    {
        const string sql = "SELECT * FROM finance.Journals WHERE TenantId = @TenantId AND Code = @Code";
        using var connection = CreateConnection(sql, new { TenantId = tenantId, Code = code });
        return await connection.QuerySingleOrDefaultAsync<Journal>(sql, new { TenantId = tenantId, Code = code });
    }

    public async Task<IEnumerable<Journal>> GetAllByTenantIdAsync(Guid tenantId)
    {
        const string sql = "SELECT * FROM finance.Journals WHERE TenantId = @TenantId ORDER BY Code ASC";
        using var connection = CreateConnection(sql, new { TenantId = tenantId });
        return await connection.QueryAsync<Journal>(sql, new { TenantId = tenantId });
    }

    public async Task<Guid> CreateAsync(Journal journal)
    {
        const string sql = @"
            INSERT INTO finance.Journals (Id, TenantId, Code, Name, Type, CreatedAt)
            VALUES (@Id, @TenantId, @Code, @Name, @Type, @CreatedAt)
            RETURNING Id";
        using var connection = CreateConnection(sql, journal);
        return await connection.ExecuteScalarAsync<Guid>(sql, journal);
    }

    public async Task<bool> UpdateAsync(Journal journal)
    {
        const string sql = @"
            UPDATE finance.Journals 
            SET Code = @Code, Name = @Name, Type = @Type, UpdatedAt = @UpdatedAt
            WHERE Id = @Id";
        using var connection = CreateConnection(sql, journal);
        var rows = await connection.ExecuteAsync(sql, journal);
        return rows > 0;
    }
}
