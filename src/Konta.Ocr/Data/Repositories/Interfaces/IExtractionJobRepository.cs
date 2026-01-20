using Konta.Ocr.Models;

namespace Konta.Ocr.Data.Repositories.Interfaces;

public interface IExtractionJobRepository
{
    Task<ExtractionJob?> GetByIdAsync(Guid id);
    Task<IEnumerable<ExtractionJob>> GetPendingJobsAsync();
    Task<Guid> CreateAsync(ExtractionJob job);
    Task<bool> UpdateStatusAsync(Guid id, JobStatus status, DocumentType type = DocumentType.Unknown, string? errorMessage = null);
    
    // Résultats
    Task<bool> SaveInvoiceResultAsync(ExtractedInvoice invoice);
    Task<bool> SaveRibResultAsync(ExtractedRib rib);
    Task<ExtractedInvoice?> GetInvoiceResultByJobIdAsync(Guid jobId);
    Task<ExtractedRib?> GetRibResultByJobIdAsync(Guid jobId);
}
