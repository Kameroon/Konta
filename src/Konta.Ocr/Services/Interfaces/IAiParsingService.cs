using Konta.Ocr.Models;

namespace Konta.Ocr.Services.Interfaces;

public interface IAiParsingService
{
    /// <summary>
    /// Analyse le texte brut et extrait les données structurées.
    /// </summary>
    Task<(DocumentType Type, object? Result)> ParseDocumentAsync(string rawText);
}
