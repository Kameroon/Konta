namespace Konta.Ocr.Services.Interfaces;

public interface IPdfService
{
    /// <summary>
    /// Extrait le texte brut d'un fichier PDF.
    /// </summary>
    Task<string> ExtractTextAsync(string filePath);
}
