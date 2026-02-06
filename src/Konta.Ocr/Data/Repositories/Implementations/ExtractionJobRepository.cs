using Dapper;
using System.Data;
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
        const string sql = "SELECT * FROM ocr.extractionjobs WHERE Id = @Id";
        using var connection = CreateConnection(sql, new { Id = id });
        return await connection.QuerySingleOrDefaultAsync<ExtractionJob>(sql, new { Id = id });
    }

    public async Task<IEnumerable<ExtractionJob>> GetAllAsync()
    {
        const string sql = "SELECT * FROM ocr.extractionjobs ORDER BY CreatedAt DESC";
        using var connection = CreateConnection(sql);
        return await connection.QueryAsync<ExtractionJob>(sql);
    }

    public async Task<IEnumerable<ExtractionJob>> GetByTenantIdAsync(Guid tenantId)
    {
        const string sql = "SELECT * FROM ocr.extractionjobs WHERE TenantId = @TenantId ORDER BY CreatedAt DESC";
        using var connection = CreateConnection(sql, new { TenantId = tenantId });
        return await connection.QueryAsync<ExtractionJob>(sql, new { TenantId = tenantId });
    }

    public async Task<IEnumerable<ExtractionJob>> GetByUserIdAsync(Guid userId)
    {
        const string sql = "SELECT * FROM ocr.extractionjobs WHERE CreatedBy = @UserId ORDER BY CreatedAt DESC";
        using var connection = CreateConnection(sql, new { UserId = userId });
        return await connection.QueryAsync<ExtractionJob>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<ExtractionJob>> GetPendingJobsAsync()
    {
        const string sql = "SELECT * FROM ocr.extractionjobs WHERE Status = @Status ORDER BY CreatedAt ASC";
        using var connection = CreateConnection(sql, new { Status = JobStatus.Pending });
        return await connection.QueryAsync<ExtractionJob>(sql, new { Status = JobStatus.Pending });
    }

    public async Task<Guid> CreateAsync(ExtractionJob job)
    {
        const string sql = @"
            INSERT INTO ocr.extractionjobs (Id, TenantId, CreatedBy, FileName, FilePath, Status, CreatedAt)
            VALUES (@Id, @TenantId, @CreatedBy, @FileName, @FilePath, @Status, @CreatedAt)
            RETURNING Id";
        using var connection = CreateConnection(sql, job);
        return await connection.ExecuteScalarAsync<Guid>(sql, job);
    }

    public async Task<bool> UpdateStatusAsync(Guid id, JobStatus status, DocumentType type = DocumentType.Unknown, string? errorMessage = null)
    {
        const string sql = @"
            UPDATE ocr.extractionjobs 
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
        const string sql = "SELECT * FROM ocr.extractedinvoices WHERE JobId = @JobId";
        using var connection = CreateConnection(sql, new { JobId = jobId });
        return await connection.QuerySingleOrDefaultAsync<ExtractedInvoice>(sql, new { JobId = jobId });
    }

    public async Task<ExtractedRib?> GetRibResultByJobIdAsync(Guid jobId)
    {
        const string sql = "SELECT * FROM ocr.extractedribs WHERE JobId = @JobId";
        using var connection = CreateConnection(sql, new { JobId = jobId });
        return await connection.QuerySingleOrDefaultAsync<ExtractedRib>(sql, new { JobId = jobId });
    }

    public async Task DeleteAsync(ExtractionJob job)
    {
        // On récupère une connexion et on l'ouvre manuellement pour garantir que la session RLS est active sur toute l'opération
        using var connection = ConnectionFactory.CreateConnection();
        if (connection.State != ConnectionState.Open) connection.Open();

        using var transaction = connection.BeginTransaction();
        try
        {
            // On supprime d'abord les résultats potentiels (contraintes FK si existantes)
            const string sqlDelResults = "DELETE FROM ocr.extractedinvoices WHERE JobId = @JobId; DELETE FROM ocr.extractedribs WHERE JobId = @JobId;";
            const string sqlDelJob = "DELETE FROM ocr.extractionjobs WHERE Id = @JobId";
            
            var parameters = new { JobId = job.Id };
            
            await connection.ExecuteAsync(sqlDelResults, parameters, transaction);
            await connection.ExecuteAsync(sqlDelJob, parameters, transaction);

            transaction.Commit();

            // Suppression du fichier physique seulement si la DB est OK
            if (File.Exists(job.FilePath))
            {
                try { File.Delete(job.FilePath); } catch { /* Log and ignore */ }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du job OCR {JobId}", job.Id);
            transaction.Rollback();
            throw;
        }
    }
}
