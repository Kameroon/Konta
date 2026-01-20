using Konta.Ocr.Data.Repositories.Interfaces;
using Konta.Ocr.Models;
using Konta.Ocr.Services.Interfaces;

namespace Konta.Ocr.Services.Implementations;

public class ExtractionService : IExtractionService
{
    private readonly IExtractionJobRepository _jobRepository;
    private readonly IPdfService _pdfService;
    private readonly IAiParsingService _aiService;
    private readonly ILogger<ExtractionService> _logger;

    public ExtractionService(
        IExtractionJobRepository jobRepository,
        IPdfService pdfService,
        IAiParsingService aiService,
        ILogger<ExtractionService> logger)
    {
        _jobRepository = jobRepository;
        _pdfService = pdfService;
        _aiService = aiService;
        _logger = logger;
    }

    public async Task ProcessJobAsync(Guid jobId)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job == null) return;

        try
        {
            _logger.LogInformation("Traitement du job OCR {JobId} ({FileName})", jobId, job.FileName);
            
            await _jobRepository.UpdateStatusAsync(jobId, JobStatus.Processing);

            // 1. Extraction du texte
            string rawText = await _pdfService.ExtractTextAsync(job.FilePath);

            // 2. Parsing IA
            var (type, result) = await _aiService.ParseDocumentAsync(rawText);

            // 3. Persistance du résultat spécifique
            if (type == DocumentType.Invoice && result is ExtractedInvoice invoice)
            {
                invoice.JobId = jobId;
                await _jobRepository.SaveInvoiceResultAsync(invoice);
            }
            else if (type == DocumentType.Rib && result is ExtractedRib rib)
            {
                rib.JobId = jobId;
                await _jobRepository.SaveRibResultAsync(rib);
            }

            // 4. Update Final
            await _jobRepository.UpdateStatusAsync(jobId, JobStatus.Completed, type);
            _logger.LogInformation("Job OCR {JobId} terminé avec succès. Type détecté : {Type}", jobId, type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Échec du job OCR {JobId}", jobId);
            await _jobRepository.UpdateStatusAsync(jobId, JobStatus.Failed, errorMessage: ex.Message);
        }
    }
}
