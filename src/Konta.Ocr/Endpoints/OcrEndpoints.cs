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
                CreatedBy = tenantContext.UserId ?? Guid.Empty,
                FileName = file.FileName,
                FilePath = filePath,
                Status = JobStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await repo.CreateAsync(job);

            return Results.Accepted($"/api/ocr/jobs/{jobId}", ApiResponse<object>.Ok(new { JobId = jobId }, "Fichier mis en file d'attente."));
        }).DisableAntiforgery().RequireAuthorization();

        // --- Delete ---
        group.MapDelete("/jobs/{id}", async (Guid id, IExtractionJobRepository repo) =>
        {
            var job = await repo.GetByIdAsync(id);
            if (job == null) return Results.NotFound(ApiResponse<object>.Fail("Job introuvable."));
            
            await repo.DeleteAsync(job);
            return Results.Ok(ApiResponse<object>.Ok(null, "Job supprimé avec succès."));
        }).RequireAuthorization();

        // --- List ---
        group.MapGet("/jobs", async (ITenantContext tenantContext, IExtractionJobRepository repo) =>
        {
            if (!tenantContext.TenantId.HasValue) return Results.Unauthorized();
            
            IEnumerable<ExtractionJob> jobs;
            if (tenantContext.IsGlobalAdmin)
            {
                jobs = await repo.GetAllAsync();
            }
            else
            {
                jobs = await repo.GetByTenantIdAsync(tenantContext.TenantId.Value);
            }
            
            return Results.Ok(ApiResponse<object>.Ok(jobs));
        }).RequireAuthorization();

        // --- User specific list ---
        group.MapGet("/jobs/mine", async (ITenantContext tenantContext, IExtractionJobRepository repo) =>
        {
            if (!tenantContext.UserId.HasValue) return Results.Unauthorized();
            var jobs = await repo.GetByUserIdAsync(tenantContext.UserId.Value);
            return Results.Ok(ApiResponse<object>.Ok(jobs));
        }).RequireAuthorization();

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

            // Retourner 200 avec null plutôt que 404 pour éviter de casser le polling frontend
            return Results.Ok(ApiResponse<object>.Ok(null));
        }).RequireAuthorization();

        // --- Download ---
        group.MapGet("/jobs/{id}/download", async (Guid id, IExtractionJobRepository repo) =>
        {
            var job = await repo.GetByIdAsync(id);
            if (job == null) return Results.NotFound(ApiResponse<object>.Fail("Job introuvable."));
            if (!File.Exists(job.FilePath)) return Results.NotFound(ApiResponse<object>.Fail("Fichier physique introuvable sur le serveur."));

            var contentType = "application/pdf";
            return Results.File(job.FilePath, contentType, job.FileName);
        }).RequireAuthorization();
    }
}
