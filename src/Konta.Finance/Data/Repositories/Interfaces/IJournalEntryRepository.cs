using Konta.Finance.Models;

namespace Konta.Finance.Data.Repositories.Interfaces;

public interface IJournalEntryRepository
{
    Task<JournalEntry?> GetByIdAsync(Guid id);
    Task<IEnumerable<JournalEntry>> GetByJournalIdAsync(Guid journalId, DateTime? startDate, DateTime? endDate);
    Task<Guid> CreateAsync(JournalEntry entry);
    Task<bool> DeleteAsync(Guid id);
    
    // Pour le Grand Livre
    Task<IEnumerable<EntryLine>> GetLinesByAccountIdAsync(Guid accountId, DateTime startDate, DateTime endDate);
}
