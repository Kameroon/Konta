using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Konta.Ocr.Models;
using Konta.Ocr.Services.Interfaces;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Registry;
using System.Text.Json;

namespace Konta.Ocr.Services.Implementations;

/// <summary>
/// Configuration pour Azure AI Document Intelligence.
/// </summary>
public class AzureAiOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ModelId { get; set; } = "prebuilt-invoice";
}

/// <summary>
/// Implémentation du service d'extraction via Azure AI Document Intelligence.
/// Intègre la résilience Polly et le mapping sémantique du POC.
/// </summary>
public class AzureAiExtractionService : IAzureAiExtractionService
{
    private readonly DocumentAnalysisClient _client;
    private readonly AzureAiOptions _options;
    private readonly ILogger<AzureAiExtractionService> _logger;
    private readonly ResiliencePipeline _resiliencePipeline;

    public AzureAiExtractionService(
        IOptions<AzureAiOptions> options,
        ResiliencePipelineProvider<string> pipelineProvider,
        ILogger<AzureAiExtractionService> logger)
    {
        _options = options.Value;
        _logger = logger;
        
        _logger.LogInformation("[DEBUG] AzureAiOptions Endpoint: '{Endpoint}', Key length: {KeyLen}", 
            _options.Endpoint, _options.ApiKey?.Length ?? 0);

        if (string.IsNullOrWhiteSpace(_options.Endpoint))
        {
            _logger.LogError("[CRITICAL] AzureAiOptions.Endpoint is EMPTY! Check appsettings.json.");
            throw new ArgumentException("AzureAiOptions.Endpoint cannot be empty.");
        }

        // Initialisation du client Azure
        _client = new DocumentAnalysisClient(new Uri(_options.Endpoint), new AzureKeyCredential(_options.ApiKey));
        
        // Utilisation du pipeline de résilience configuré dans KONTA
        _resiliencePipeline = pipelineProvider.GetPipeline(Konta.Shared.Resilience.ResilienceConstants.DefaultPolicy);
    }

    public async Task<ExtractedInvoice?> ExtractInvoiceAsync(Stream pdfStream, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Début de l'extraction Azure AI via le modèle {ModelId}", _options.ModelId);

        try
        {
            // Copie en mémoire pour supporter les retries Polly
            using var ms = new MemoryStream();
            await pdfStream.CopyToAsync(ms, cancellationToken);

            return await _resiliencePipeline.ExecuteAsync(async (ct) =>
            {
                ms.Position = 0;
                
                // Appel à l'API Azure Document Intelligence
                var operation = await _client.AnalyzeDocumentAsync(
                    WaitUntil.Completed, 
                    _options.ModelId, 
                    ms, 
                    cancellationToken: ct);

                var result = operation.Value;
                var document = result.Documents.FirstOrDefault();

                if (document == null)
                {
                    _logger.LogWarning("Azure AI n'a détecté aucun document structuré dans le fichier.");
                    return null;
                }

                _logger.LogInformation("Extraction Azure réussie avec un score de confiance de {Confidence:P2}", document.Confidence);

                return MapToExtractedInvoice(document, result.Content);
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur fatale lors de l'appel à Azure AI Document Intelligence.");
            return null;
        }
    }

    private ExtractedInvoice MapToExtractedInvoice(AnalyzedDocument document, string rawContent)
    {
        // Mapping sémantique avec gestion des alias (inspiré du POC)
        var invoiceNumber = GetField(document, "InvoiceId", "InvoiceNumber", "InvoiceNo");
        var vendorName = GetField(document, "VendorName", "IssuerName");
        var vendorTaxId = GetField(document, "VendorTaxId", "Siret", "Siren");
        var customerTaxId = GetField(document, "CustomerTaxId", "RecipientTaxId");
        var vendorVat = GetField(document, "VendorVatNumber", "TaxId");
        var totalAmountHt = GetAmount(document, "SubTotal", "TotalExclTax");
        var totalAmountTtc = GetAmount(document, "InvoiceTotal", "TotalInclTax", "AmountDue");
        var vatAmount = GetAmount(document, "TotalTax", "VatAmount");
        var invoiceDate = GetDate(document, "InvoiceDate", "Date");
        var dueDate = GetDate(document, "DueDate", "PaymentDueDate");
        var currency = GetField(document, "CurrencyCode", "Currency");

        // Construction de l'entité de domaine KONTA
        var invoice = new ExtractedInvoice
        {
            Id = Guid.NewGuid(),
            VendorName = vendorName,
            VendorSiret = ExtractSiren(vendorTaxId),
            CustomerSiret = ExtractSiren(customerTaxId),
            VendorVatNumber = vendorVat,
            InvoiceNumber = invoiceNumber,
            InvoiceDate = invoiceDate,
            DueDate = dueDate,
            TotalAmountHt = totalAmountHt,
            TotalAmountTtc = totalAmountTtc,
            VatAmount = vatAmount,
            Currency = currency ?? "EUR",
            ConfidenceScore = (int)(document.Confidence * 100),
            RawJson = JsonSerializer.Serialize(document.Fields.ToDictionary(k => k.Key, v => v.Value.Content)),
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogDebug("Mapping Azure -> Konta terminé pour la facture {InvoiceNumber}", invoice.InvoiceNumber);
        return invoice;
    }

    private string? GetField(AnalyzedDocument doc, params string[] aliases)
    {
        foreach (var alias in aliases)
        {
            if (doc.Fields.TryGetValue(alias, out var field)) return field.Content;
        }
        return null;
    }

    private decimal? GetAmount(AnalyzedDocument doc, params string[] aliases)
    {
        foreach (var alias in aliases)
        {
            if (doc.Fields.TryGetValue(alias, out var field))
            {
                if (field.FieldType == DocumentFieldType.Double) return (decimal?)field.Value.AsDouble();
                if (field.FieldType == DocumentFieldType.Currency) return (decimal?)field.Value.AsCurrency().Amount;
                
                // Fallback de parsing manuel si le type n'est pas détecté
                if (decimal.TryParse(field.Content.Replace(" ", "").Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var val))
                    return val;
            }
        }
        return null;
    }

    private DateOnly? GetDate(AnalyzedDocument doc, params string[] aliases)
    {
        foreach (var alias in aliases)
        {
            if (doc.Fields.TryGetValue(alias, out var field))
            {
                if (field.FieldType == DocumentFieldType.Date) 
                    return DateOnly.FromDateTime(field.Value.AsDate().DateTime);
            }
        }
        return null;
    }

    private string? ExtractSiren(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var digits = new string(input.Where(char.IsDigit).ToArray());
        if (digits.Length >= 9) return digits[..9]; // Retourne le SIREN (9 premiers chiffres)
        return digits;
    }
}
