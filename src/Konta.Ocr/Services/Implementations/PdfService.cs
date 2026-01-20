using Konta.Ocr.Services.Interfaces;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using System.Text;

namespace Konta.Ocr.Services.Implementations;

public class PdfService : IPdfService
{
    private readonly ILogger<PdfService> _logger;

    public PdfService(ILogger<PdfService> logger)
    {
        _logger = logger;
    }

    public Task<string> ExtractTextAsync(string filePath)
    {
        try
        {
            _logger.LogInformation("Extraction du texte du fichier PDF : {FilePath}", filePath);
            
            using var document = PdfDocument.Open(filePath);
            var sb = new StringBuilder();

            foreach (var page in document.GetPages())
            {
                sb.AppendLine($"--- Page {page.Number} ---");
                sb.AppendLine(page.Text);
            }

            return Task.FromResult(sb.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'extraction du texte du PDF : {FilePath}", filePath);
            throw;
        }
    }
}
