using Konta.Ocr.Models;

namespace Konta.Ocr.Services.Interfaces;

/// <summary>
/// Service d'extraction de données via Azure AI Document Intelligence.
/// </summary>
public interface IAzureAiExtractionService
{
    /// <summary>
    /// Extrait les données structurées d'une facture à partir d'un flux PDF.
    /// </summary>
    /// <param name="pdfStream">Flux du fichier PDF.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Résultat d'extraction formaté pour le domaine Konta.</returns>
    Task<ExtractedInvoice?> ExtractInvoiceAsync(Stream pdfStream, CancellationToken cancellationToken = default);
}
