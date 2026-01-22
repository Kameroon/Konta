using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Konta.Ocr.Models;
using Konta.Ocr.Data.Repositories.Interfaces;
using Konta.Shared.Data;
using Konta.Shared.Responses;

namespace Konta.Ocr.Endpoints;

public static class OcrEndpoints
{
    public static void MapOcrEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ocr").WithTags("OCR & Extraction");

        // --- Upload ---
        group.MapPost("/upload", async (IFormFile file, ITenantContext tenantContext, IExtractionJobRepository repo, IConfiguration config) =>
        {
            if (!tenantContext.TenantId.HasValue) return Results.Unauthorized();
            if (file == null || file.Length == 0) return Results.BadRequest("Fichier manquant.");
            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) return Results.BadRequest("Seuls les fichiers PDF sont acceptés.");

            var storagePath = config["Storage:LocalPath"] ?? "storage/ocr";
            if (!Directory.Exists(storagePath)) Directory.CreateDirectory(storagePath);

            var jobId = Guid.NewGuid();
            var fileName = $"{jobId}_{file.FileName}";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), storagePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var job = new ExtractionJob
            {
                Id = jobId,
                TenantId = tenantContext.TenantId.Value,
                FileName = file.FileName,
                FilePath = filePath,
                Status = JobStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await repo.CreateAsync(job);

            return Results.Accepted($"/api/ocr/jobs/{jobId}", ApiResponse<object>.Ok(new { JobId = jobId }, "Fichier mis en file d'attente."));
        }).DisableAntiforgery().RequireAuthorization();

        // --- Status ---
        group.MapGet("/jobs/{id}", async (Guid id, IExtractionJobRepository repo) =>
        {
            var job = await repo.GetByIdAsync(id);
            return job is not null ? Results.Ok(ApiResponse<object>.Ok(job)) : Results.NotFound(ApiResponse<object>.Fail("Job introuvable."));
        }).RequireAuthorization();

        // --- Results ---
        group.MapGet("/results/{jobId}", async (Guid jobId, IExtractionJobRepository repo) =>
        {
            var job = await repo.GetByIdAsync(jobId);
            if (job == null) return Results.NotFound(ApiResponse<object>.Fail("Job introuvable."));
            if (job.Status != JobStatus.Completed) return Results.BadRequest(ApiResponse<object>.Fail("Le job n'est pas encore terminé."));

            if (job.DetectedType == DocumentType.Invoice)
            {
                var result = await repo.GetInvoiceResultByJobIdAsync(jobId);
                return Results.Ok(ApiResponse<object>.Ok(result));
            }
            else if (job.DetectedType == DocumentType.Rib)
            {
                var result = await repo.GetRibResultByJobIdAsync(jobId);
                return Results.Ok(ApiResponse<object>.Ok(result));
            }

            return Results.NotFound(ApiResponse<object>.Fail("Aucun résultat structuré disponible pour ce type."));
        }).RequireAuthorization();
    }
}
