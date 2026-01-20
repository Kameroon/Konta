using Dapper;
using Konta.Finance.Data.Repositories.Interfaces;
using Konta.Finance.Models;
using Konta.Shared.Data;
using Konta.Shared.Data.Repositories;

namespace Konta.Finance.Data.Repositories.Implementations;

public class AccountRepository : BaseRepository<AccountRepository>, IAccountRepository
{
    public AccountRepository(IDbConnectionFactory connectionFactory, ILogger<AccountRepository> logger) 
        : base(connectionFactory, logger)
    {
    }

    public async Task<Account?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT * FROM finance.Accounts WHERE Id = @Id";
        using var connection = CreateConnection(sql, new { Id = id });
        return await connection.QuerySingleOrDefaultAsync<Account>(sql, new { Id = id });
    }

    public async Task<Account?> GetByCodeAsync(Guid tenantId, string code)
    {
        const string sql = "SELECT * FROM finance.Accounts WHERE TenantId = @TenantId AND Code = @Code";
        using var connection = CreateConnection(sql, new { TenantId = tenantId, Code = code });
        return await connection.QuerySingleOrDefaultAsync<Account>(sql, new { TenantId = tenantId, Code = code });
    }

    public async Task<IEnumerable<Account>> GetAllByTenantIdAsync(Guid tenantId)
    {
        const string sql = "SELECT * FROM finance.Accounts WHERE TenantId = @TenantId ORDER BY Code ASC";
        using var connection = CreateConnection(sql, new { TenantId = tenantId });
        return await connection.QueryAsync<Account>(sql, new { TenantId = tenantId });
    }

    public async Task<Guid> CreateAsync(Account account)
    {
        const string sql = @"
            INSERT INTO finance.Accounts (Id, TenantId, Code, Name, Type, ParentId, FullPath, CreatedAt)
            VALUES (@Id, @TenantId, @Code, @Name, @Type, @ParentId, @FullPath, @CreatedAt)
            RETURNING Id";
        using var connection = CreateConnection(sql, account);
        return await connection.ExecuteScalarAsync<Guid>(sql, account);
    }

    public async Task<bool> UpdateAsync(Account account)
    {
        const string sql = @"
            UPDATE finance.Accounts 
            SET Code = @Code, Name = @Name, Type = @Type, ParentId = @ParentId, 
                FullPath = @FullPath, UpdatedAt = @UpdatedAt
            WHERE Id = @Id";
        using var connection = CreateConnection(sql, account);
        var rows = await connection.ExecuteAsync(sql, account);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        const string sql = "DELETE FROM finance.Accounts WHERE Id = @Id";
        using var connection = CreateConnection(sql, new { Id = id });
        var rows = await connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}
