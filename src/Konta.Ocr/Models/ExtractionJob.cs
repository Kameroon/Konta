using Konta.Shared.Models;

namespace Konta.Ocr.Models;

public enum JobStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}

public enum DocumentType
{
    Unknown = 0,
    Invoice = 1,
    ExpenseReport = 2,
    Rib = 3
}

/// <summary>
/// Représente un job d'extraction OCR.
/// </summary>
public class ExtractionJob : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid CreatedBy { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public DocumentType DetectedType { get; set; } = DocumentType.Unknown;
    public string? ErrorMessage { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
