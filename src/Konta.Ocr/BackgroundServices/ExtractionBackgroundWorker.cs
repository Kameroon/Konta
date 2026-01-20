using Konta.Ocr.Data.Repositories.Interfaces;
using Konta.Ocr.Services.Interfaces;

namespace Konta.Ocr.BackgroundServices;

/// <summary>
/// Worker d'arrière-plan traitant les jobs d'extraction en file d'attente.
/// </summary>
public class ExtractionBackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExtractionBackgroundWorker> _logger;

    public ExtractionBackgroundWorker(IServiceProvider serviceProvider, ILogger<ExtractionBackgroundWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BackgroundWorker OCR démarré.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var jobRepository = scope.ServiceProvider.GetRequiredService<IExtractionJobRepository>();
                var extractionService = scope.ServiceProvider.GetRequiredService<IExtractionService>();

                var pendingJobs = await jobRepository.GetPendingJobsAsync();

                foreach (var job in pendingJobs)
                {
                    if (stoppingToken.IsCancellationRequested) break;
                    
                    await extractionService.ProcessJobAsync(job.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dans le cycle du BackgroundWorker OCR.");
            }

            // Attendre avant le prochain poll (ex: 5 secondes)
            await Task.Delay(5000, stoppingToken);
        }

        _logger.LogInformation("BackgroundWorker OCR arrêté.");
    }
}
