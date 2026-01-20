using Dapper;
using Konta.Finance.Data.Repositories.Interfaces;
using Konta.Finance.Models;
using Konta.Shared.Data;
using Konta.Shared.Data.Repositories;
using System.Data;

namespace Konta.Finance.Data.Repositories.Implementations;

public class JournalEntryRepository : BaseRepository<JournalEntryRepository>, IJournalEntryRepository
{
    public JournalEntryRepository(IDbConnectionFactory connectionFactory, ILogger<JournalEntryRepository> logger) 
        : base(connectionFactory, logger)
    {
    }

    public async Task<JournalEntry?> GetByIdAsync(Guid id)
    {
        const string sqlEntry = "SELECT * FROM JournalEntries WHERE Id = @Id";
        const string sqlLines = "SELECT * FROM EntryLines WHERE EntryId = @Id";

        using var connection = CreateConnection(sqlEntry, new { Id = id });
        var entry = await connection.QuerySingleOrDefaultAsync<JournalEntry>(sqlEntry, new { Id = id });
        
        if (entry != null)
        {
            entry.Lines = (await connection.QueryAsync<EntryLine>(sqlLines, new { Id = id })).ToList();
        }

        return entry;
    }

    public async Task<IEnumerable<JournalEntry>> GetByJournalIdAsync(Guid journalId, DateTime? startDate, DateTime? endDate)
    {
        var sql = "SELECT * FROM JournalEntries WHERE JournalId = @JournalId";
        if (startDate.HasValue) sql += " AND EntryDate >= @StartDate";
        if (endDate.HasValue) sql += " AND EntryDate <= @EndDate";
        sql += " ORDER BY EntryDate DESC";

        using var connection = CreateConnection(sql, new { JournalId = journalId, StartDate = startDate, EndDate = endDate });
        return await connection.QueryAsync<JournalEntry>(sql, new { JournalId = journalId, StartDate = startDate, EndDate = endDate });
    }

    public async Task<Guid> CreateAsync(JournalEntry entry)
    {
        // On utilise une transaction manuelle car Dapper n'en gère pas par défaut sur plusieurs appels
        using var connection = ConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string sqlEntry = @"
                INSERT INTO JournalEntries (Id, TenantId, JournalId, EntryDate, Reference, Description, CreatedAt)
                VALUES (@Id, @TenantId, @JournalId, @EntryDate, @Reference, @Description, @CreatedAt)";
            
            await connection.ExecuteAsync(sqlEntry, entry, transaction);

            const string sqlLine = @"
                INSERT INTO EntryLines (Id, EntryId, AccountId, Label, Debit, Credit, CreatedAt)
                VALUES (@Id, @EntryId, @AccountId, @Label, @Debit, @Credit, @CreatedAt)";

            foreach (var line in entry.Lines)
            {
                line.EntryId = entry.Id;
                await connection.ExecuteAsync(sqlLine, line, transaction);
            }

            transaction.Commit();
            return entry.Id;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = ConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            await connection.ExecuteAsync("DELETE FROM EntryLines WHERE EntryId = @Id", new { Id = id }, transaction);
            var rows = await connection.ExecuteAsync("DELETE FROM JournalEntries WHERE Id = @Id", new { Id = id }, transaction);
            
            transaction.Commit();
            return rows > 0;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<EntryLine>> GetLinesByAccountIdAsync(Guid accountId, DateTime startDate, DateTime endDate)
    {
        const string sql = @"
            SELECT l.* 
            FROM EntryLines l
            JOIN JournalEntries e ON l.EntryId = e.Id
            WHERE l.AccountId = @AccountId 
              AND e.EntryDate >= @StartDate 
              AND e.EntryDate <= @EndDate
            ORDER BY e.EntryDate ASC";
        
        using var connection = CreateConnection(sql, new { AccountId = accountId, StartDate = startDate, EndDate = endDate });
        return await connection.QueryAsync<EntryLine>(sql, new { AccountId = accountId, StartDate = startDate, EndDate = endDate });
    }
}
