using Konta.Ocr.Data.Repositories.Interfaces;
using Konta.Ocr.Models;
using Konta.Ocr.Services.Interfaces;

namespace Konta.Ocr.Services.Implementations;

public class ExtractionService : IExtractionService
{
    private readonly IExtractionJobRepository _jobRepository;
    private readonly IPdfService _pdfService;
    private readonly IAiParsingService _aiService;
    private readonly IAzureAiExtractionService _azureAiService;
    private readonly ILogger<ExtractionService> _logger;

    public ExtractionService(
        IExtractionJobRepository jobRepository,
        IPdfService pdfService,
        IAiParsingService aiService,
        IAzureAiExtractionService azureAiService,
        ILogger<ExtractionService> logger)
    {
        _jobRepository = jobRepository;
        _pdfService = pdfService;
        _aiService = aiService;
        _azureAiService = azureAiService;
        _logger = logger;
    }

    public async Task ProcessJobAsync(Guid jobId)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job == null) return;

        try
        {
            _logger.LogInformation("Traitement SMART du job OCR {JobId} ({FileName})", jobId, job.FileName);
            await _jobRepository.UpdateStatusAsync(jobId, JobStatus.Processing);

            ExtractedInvoice? finalResult = null;
            DocumentType type = DocumentType.Unknown;

            // 1. Charger le fichier en flux
            if (!File.Exists(job.FilePath)) throw new FileNotFoundException("Fichier non trouvé", job.FilePath);
            
            using (var stream = new FileStream(job.FilePath, FileMode.Open, FileAccess.Read))
            {
                // -- MOTEUR 1 : AZURE AI (PRIMARY) --
                _logger.LogInformation("[Step 1] Tentative via Azure AI Document Intelligence...");
                finalResult = await _azureAiService.ExtractInvoiceAsync(stream);
                
                if (finalResult != null)
                {
                    type = DocumentType.Invoice;
                    _logger.LogInformation("[Step 1] Succès Azure AI (Confiance: {Conf}%)", finalResult.ConfidenceScore);
                }
                else
                {
                    _logger.LogWarning("[Step 1] Échec Azure AI ou Document non reconnu. Passage au Moteur 2.");
                    
                    // -- MOTEUR 2 & 3 : GEMINI + REGEX (FALLBACK) --
                    _logger.LogInformation("[Step 2] Extraction du texte natif pour Fallback...");
                    string rawText = await _pdfService.ExtractTextAsync(job.FilePath);
                    
                    var (fallbackType, fallbackResult) = await _aiService.ParseDocumentAsync(rawText);
                    type = fallbackType;
                    
                    if (fallbackType == DocumentType.Invoice && fallbackResult is ExtractedInvoice invoice)
                    {
                        finalResult = invoice;
                        _logger.LogInformation("[Step 2] Succès Fallback {Source}", invoice.ConfidenceScore > 50 ? "Gemini" : "Regex");
                    }
                    else if (fallbackType == DocumentType.Rib && fallbackResult is ExtractedRib rib)
                    {
                        rib.JobId = jobId;
                        await _jobRepository.SaveRibResultAsync(rib);
                    }
                }
            }

            // 3. Persistance du résultat final (Invoice)
            if (type == DocumentType.Invoice && finalResult != null)
            {
                finalResult.JobId = jobId;
                await _jobRepository.SaveInvoiceResultAsync(finalResult);
            }

            // 4. Update Final
            await _jobRepository.UpdateStatusAsync(jobId, JobStatus.Completed, type);
            _logger.LogInformation("Job OCR {JobId} terminé. Type: {Type}. Source: {Source}", 
                jobId, type, finalResult?.ConfidenceScore > 80 ? "AzureAI" : "Hybrid/Fallback");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Échec critique du job OCR {JobId}", jobId);
            await _jobRepository.UpdateStatusAsync(jobId, JobStatus.Failed, errorMessage: ex.Message);
        }
    }
}
