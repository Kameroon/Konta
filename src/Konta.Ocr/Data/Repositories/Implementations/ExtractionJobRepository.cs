using Dapper;
using Konta.Ocr.Data.Repositories.Interfaces;
using Konta.Ocr.Models;
using Konta.Shared.Data;
using Konta.Shared.Data.Repositories;

namespace Konta.Ocr.Data.Repositories.Implementations;

public class ExtractionJobRepository : BaseRepository<ExtractionJobRepository>, IExtractionJobRepository
{
    public ExtractionJobRepository(IDbConnectionFactory connectionFactory, ILogger<ExtractionJobRepository> logger) 
        : base(connectionFactory, logger)
    {
    }

    public async Task<ExtractionJob?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT * FROM ocr.ExtractionJobs WHERE Id = @Id";
        using var connection = CreateConnection(sql, new { Id = id });
        return await connection.QuerySingleOrDefaultAsync<ExtractionJob>(sql, new { Id = id });
    }

    public async Task<IEnumerable<ExtractionJob>> GetAllAsync()
    {
        const string sql = "SELECT * FROM ocr.ExtractionJobs ORDER BY CreatedAt DESC";
        using var connection = CreateConnection(sql);
        return await connection.QueryAsync<ExtractionJob>(sql);
    }

    public async Task<IEnumerable<ExtractionJob>> GetByTenantIdAsync(Guid tenantId)
    {
        const string sql = "SELECT * FROM ocr.ExtractionJobs WHERE TenantId = @TenantId ORDER BY CreatedAt DESC";
        using var connection = CreateConnection(sql, new { TenantId = tenantId });
        return await connection.QueryAsync<ExtractionJob>(sql, new { TenantId = tenantId });
    }

    public async Task<IEnumerable<ExtractionJob>> GetByUserIdAsync(Guid userId)
    {
        const string sql = "SELECT * FROM ocr.ExtractionJobs WHERE CreatedBy = @UserId ORDER BY CreatedAt DESC";
        using var connection = CreateConnection(sql, new { UserId = userId });
        return await connection.QueryAsync<ExtractionJob>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<ExtractionJob>> GetPendingJobsAsync()
    {
        const string sql = "SELECT * FROM ocr.ExtractionJobs WHERE Status = @Status ORDER BY CreatedAt ASC";
        using var connection = CreateConnection(sql, new { Status = JobStatus.Pending });
        return await connection.QueryAsync<ExtractionJob>(sql, new { Status = JobStatus.Pending });
    }

    public async Task<Guid> CreateAsync(ExtractionJob job)
    {
        const string sql = @"
            INSERT INTO ocr.ExtractionJobs (Id, TenantId, CreatedBy, FileName, FilePath, Status, CreatedAt)
            VALUES (@Id, @TenantId, @CreatedBy, @FileName, @FilePath, @Status, @CreatedAt)
            RETURNING Id";
        using var connection = CreateConnection(sql, job);
        return await connection.ExecuteScalarAsync<Guid>(sql, job);
    }

    public async Task<bool> UpdateStatusAsync(Guid id, JobStatus status, DocumentType type = DocumentType.Unknown, string? errorMessage = null)
    {
        const string sql = @"
            UPDATE ocr.ExtractionJobs 
            SET Status = @Status, DetectedType = @DetectedType, 
                ErrorMessage = @ErrorMessage, ProcessedAt = @ProcessedAt, UpdatedAt = @UpdatedAt 
            WHERE Id = @Id";
        
        var parameters = new { 
            Id = id, 
            Status = status, 
            DetectedType = type, 
            ErrorMessage = errorMessage, 
            ProcessedAt = (status == JobStatus.Completed || status == JobStatus.Failed) ? DateTime.UtcNow : (DateTime?)null,
            UpdatedAt = DateTime.UtcNow
        };

        using var connection = CreateConnection(sql, parameters);
        var rows = await connection.ExecuteAsync(sql, parameters);
        return rows > 0;
    }

    public async Task<bool> SaveInvoiceResultAsync(ExtractedInvoice invoice)
    {
        const string sql = @"
            INSERT INTO ocr.ExtractedInvoices (Id, JobId, VendorName, InvoiceNumber, InvoiceDate, TotalAmountHt, TotalAmountTtc, VatAmount, Currency, RawJson, CreatedAt)
            VALUES (@Id, @JobId, @VendorName, @InvoiceNumber, @InvoiceDate, @TotalAmountHt, @TotalAmountTtc, @VatAmount, @Currency, @RawJson::jsonb, @CreatedAt)";
        using var connection = CreateConnection(sql, invoice);
        var rows = await connection.ExecuteAsync(sql, invoice);
        return rows > 0;
    }

    public async Task<bool> SaveRibResultAsync(ExtractedRib rib)
    {
        const string sql = @"
            INSERT INTO ocr.ExtractedRibs (Id, JobId, BankName, Iban, Bic, AccountHolder, CreatedAt)
            VALUES (@Id, @JobId, @BankName, @Iban, @Bic, @AccountHolder, @CreatedAt)";
        using var connection = CreateConnection(sql, rib);
        var rows = await connection.ExecuteAsync(sql, rib);
        return rows > 0;
    }

    public async Task<ExtractedInvoice?> GetInvoiceResultByJobIdAsync(Guid jobId)
    {
        const string sql = "SELECT * FROM ocr.ExtractedInvoices WHERE JobId = @JobId";
        using var connection = CreateConnection(sql, new { JobId = jobId });
        return await connection.QuerySingleOrDefaultAsync<ExtractedInvoice>(sql, new { JobId = jobId });
    }

    public async Task<ExtractedRib?> GetRibResultByJobIdAsync(Guid jobId)
    {
        const string sql = "SELECT * FROM ocr.ExtractedRibs WHERE JobId = @JobId";
        using var connection = CreateConnection(sql, new { JobId = jobId });
        return await connection.QuerySingleOrDefaultAsync<ExtractedRib>(sql, new { JobId = jobId });
    }

    public async Task DeleteAsync(ExtractionJob job)
    {
        // On supprime d'abord les résultats potentiels (contraintes FK si existantes)
        const string sqlDelResults = "DELETE FROM ocr.ExtractedInvoices WHERE JobId = @Id; DELETE FROM ocr.ExtractedRibs WHERE JobId = @Id;";
        const string sqlDelJob = "DELETE FROM ocr.ExtractionJobs WHERE Id = @Id";
        
        using var connection = CreateConnection(sqlDelJob, new { Id = job.Id });
        await connection.ExecuteAsync(sqlDelResults, new { Id = job.Id });
        await connection.ExecuteAsync(sqlDelJob, new { Id = job.Id });

        // Note: Suppression du fichier physique recommandée en production
        if (File.Exists(job.FilePath))
        {
            try { File.Delete(job.FilePath); } catch { /* Log and ignore */ }
        }
    }
}
