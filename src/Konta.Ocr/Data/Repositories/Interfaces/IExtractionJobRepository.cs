using Konta.Ocr.Models;

namespace Konta.Ocr.Data.Repositories.Interfaces;

public interface IExtractionJobRepository
{
    Task<ExtractionJob?> GetByIdAsync(Guid id);
    Task<IEnumerable<ExtractionJob>> GetAllAsync();
    Task<IEnumerable<ExtractionJob>> GetByTenantIdAsync(Guid tenantId);
    Task<IEnumerable<ExtractionJob>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<ExtractionJob>> GetPendingJobsAsync();
    Task<Guid> CreateAsync(ExtractionJob job);
    Task<bool> UpdateStatusAsync(Guid id, JobStatus status, DocumentType type = DocumentType.Unknown, string? errorMessage = null);
    Task DeleteAsync(ExtractionJob job);
    
    // Résultats
    Task<bool> SaveInvoiceResultAsync(ExtractedInvoice invoice);
    Task<bool> SaveRibResultAsync(ExtractedRib rib);
    Task<ExtractedInvoice?> GetInvoiceResultByJobIdAsync(Guid jobId);
    Task<ExtractedRib?> GetRibResultByJobIdAsync(Guid jobId);
}
