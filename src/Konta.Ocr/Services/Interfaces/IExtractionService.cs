namespace Konta.Ocr.Services.Interfaces;

public interface IExtractionService
{
    /// <summary>
    /// Lance le processus d'extraction pour un job donné.
    /// </summary>
    Task ProcessJobAsync(Guid jobId);
}
