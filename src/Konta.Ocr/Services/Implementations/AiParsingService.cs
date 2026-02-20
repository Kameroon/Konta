using Konta.Ocr.Models;
using Konta.Ocr.Services.Interfaces;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Konta.Ocr.Services.Implementations;

public class AiParsingService : IAiParsingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AiParsingService> _logger;
    private readonly IAzureAiExtractionService _azureAiService;

    public AiParsingService(
        HttpClient httpClient, 
        IConfiguration configuration, 
        ILogger<AiParsingService> logger,
        IAzureAiExtractionService azureAiService)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _azureAiService = azureAiService;
    }

    public async Task<(DocumentType Type, object? Result)> ParseDocumentAsync(string rawText)
    {
        // Note: rawText est fourni pour Gemini/Regex, mais Azure AI travaille sur le Stream (géré dans ExtractionService)
        _logger.LogInformation("Orchestration de l'extraction hybride...");
        
        // Le flux de décision est géré dans ExtractionService. ProcessJobAsync appelle maintenant l'IA.
        // On garde cette méthode pour la compatibilité avec l'interface, mais la logique principale 
        // de secours Gemini/Regex est implémentée ici.
        
        return await ParseWithGeminiAsync(rawText);
    }

    public async Task<(DocumentType Type, object? Result)> ParseWithGeminiAsync(string rawText)
    {
        _logger.LogInformation("Tentative d'extraction via Gemini 1.5 Flash (Validation/Fallback)");

        string apiKey = (_configuration["AiSettings:GeminiApiKey"] ?? "").Trim();
        string model = (_configuration["AiSettings:ModelName"] ?? "gemini-1.5-flash").Trim();

        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY")
        {
            _logger.LogWarning("Clé Gemini non configurée. Passage aux Regex direct.");
            return ParseWithRegex(rawText);
        }

        try
        {
            var prompt = @"Tu es un expert comptable spécialisé dans l'analyse de documents. 
Analyse le texte suivant extrait d'un PDF et identifie s'il s'agit d'une FACTURE (Invoice) ou d'un RIB (Bank Details).
Retourne EXCLUSIVEMENT un objet JSON structuré comme suit :

Pour une FACTURE:
{ ""type"": ""Invoice"", ""data"": { ""vendor"": ""nom"", ""siret"": ""14 chiffres émetteur"", ""customerSiret"": ""14 chiffres destinataire"", ""number"": ""numéro"", ""date"": ""YYYY-MM-DD"", ""dueDate"": ""YYYY-MM-DD"", ""totalHt"": 0.0, ""totalTtc"": 0.0, ""vat"": 0.0, ""currency"": ""EUR"" } }

Pour un RIB:
{ ""type"": ""Rib"", ""data"": { ""bank"": ""nom"", ""iban"": ""IBAN sans espaces"", ""bic"": ""BIC"", ""holder"": ""nom du titulaire"" } }

TEXTE À ANALYSER:
" + rawText;

            var requestBody = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
            
            var response = await _httpClient.PostAsJsonAsync(url, requestBody);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erreur API Gemini ({Status}) : {Error}", response.StatusCode, errorBody);
                return ParseWithRegex(rawText);
            }

            var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            var jsonString = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
            if (string.IsNullOrEmpty(jsonString)) 
            {
                _logger.LogWarning("Réponse Gemini vide ou malformée.");
                return ParseWithRegex(rawText);
            }

            jsonString = Regex.Replace(jsonString, "```json|```", "").Trim();
            var resultDoc = JsonSerializer.Deserialize<AiResultWrapper>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (resultDoc?.Type == "Invoice" && resultDoc.Data is JsonElement invoiceData)
            {
                var invoice = new ExtractedInvoice
                {
                    Id = Guid.NewGuid(),
                    VendorName = invoiceData.GetProperty("vendor").GetString() ?? "Inconnu",
                    VendorSiret = invoiceData.TryGetProperty("siret", out var s) ? s.GetString() : null,
                    CustomerSiret = invoiceData.TryGetProperty("customerSiret", out var cs) ? cs.GetString() : null,
                    InvoiceNumber = invoiceData.GetProperty("number").GetString() ?? "Inconnu",
                    InvoiceDate = DateOnly.FromDateTime(DateTime.TryParse(invoiceData.GetProperty("date").GetString(), out var d) ? d : DateTime.UtcNow),
                    DueDate = invoiceData.TryGetProperty("dueDate", out var dd) && DateTime.TryParse(dd.GetString(), out var dud) ? DateOnly.FromDateTime(dud) : null,
                    TotalAmountHt = invoiceData.GetProperty("totalHt").GetDecimal(),
                    TotalAmountTtc = invoiceData.GetProperty("totalTtc").GetDecimal(),
                    VatAmount = invoiceData.GetProperty("vat").GetDecimal(),
                    Currency = invoiceData.GetProperty("currency").GetString() ?? "EUR",
                    ConfidenceScore = 80, // Score arbitraire pour Gemini seul
                    RawJson = jsonString,
                    CreatedAt = DateTime.UtcNow
                };

                // Amélioration via Regex (Validation croisée)
                var regexResult = ParseWithRegex(rawText);
                if (regexResult.Type == DocumentType.Invoice && regexResult.Result is ExtractedInvoice regInv)
                {
                    if (string.IsNullOrEmpty(invoice.VendorSiret)) invoice.VendorSiret = regInv.VendorSiret;
                    if (invoice.TotalAmountTtc == 0) invoice.TotalAmountTtc = regInv.TotalAmountTtc;
                }

                return (DocumentType.Invoice, invoice);
            }
            else if (resultDoc?.Type == "Rib" && resultDoc.Data is JsonElement ribData)
            {
                return (DocumentType.Rib, new ExtractedRib
                {
                    Id = Guid.NewGuid(),
                    BankName = ribData.GetProperty("bank").GetString() ?? "Inconnue",
                    Iban = ribData.GetProperty("iban").GetString() ?? "",
                    Bic = ribData.GetProperty("bic").GetString() ?? "",
                    AccountHolder = ribData.GetProperty("holder").GetString() ?? ""
                });
            }

            _logger.LogWarning("Type de document non identifié par l'IA.");
            return (DocumentType.Unknown, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur Gemini. Fallback Regex.");
            return ParseWithRegex(rawText);
        }
    }

    public (DocumentType Type, object? Result) ParseWithRegex(string rawText)
    {
        _logger.LogDebug("Exécution de la couche de validation Regex (POC Logic)");
        
        bool isInvoice = rawText.Contains("Facture", StringComparison.OrdinalIgnoreCase) || rawText.Contains("TTC", StringComparison.OrdinalIgnoreCase);

        if (isInvoice)
        {
            // Regex robustes du POC
            var siretMatch = Regex.Match(rawText, @"\d{14}|\d{3}\s\d{3}\s\d{3}\s\d{5}");
            var amountMatch = Regex.Match(rawText, @"(\d+[.,\s]*\d{2})\s*(?:€|EUR)", RegexOptions.IgnoreCase);
            
            var invoice = new ExtractedInvoice
            {
                Id = Guid.NewGuid(),
                VendorSiret = siretMatch.Success ? siretMatch.Value.Replace(" ", "") : null,
                TotalAmountTtc = amountMatch.Success ? decimal.Parse(amountMatch.Groups[1].Value.Replace(",", ".").Replace(" ", "")) : 0,
                ConfidenceScore = 50,
                RawJson = "{\"source\": \"regex-poc-fallback\"}",
                CreatedAt = DateTime.UtcNow
            };

            return (DocumentType.Invoice, invoice);
        }
        
        bool isRib = rawText.Contains("RIB", StringComparison.OrdinalIgnoreCase) 
                  || rawText.Contains("IBAN", StringComparison.OrdinalIgnoreCase)
                  || rawText.Contains("BIC", StringComparison.OrdinalIgnoreCase);

        if (isRib)
        {
            var ibanMatch = Regex.Match(rawText, @"(?:IBAN)\s*[:\s]*([A-Z]{2}\d{2}[A-Z0-9\s]{10,30})", RegexOptions.IgnoreCase);
            var bicMatch = Regex.Match(rawText, @"(?:BIC|SWIFT)\s*[:\s]*([A-Z0-9]{8,11})", RegexOptions.IgnoreCase);

            return (DocumentType.Rib, new ExtractedRib 
            { 
                Id = Guid.NewGuid(), 
                BankName = "Détecté via Regex",
                Iban = ibanMatch.Success ? ibanMatch.Groups[1].Value.Replace(" ", "") : "",
                Bic = bicMatch.Success ? bicMatch.Groups[1].Value.Trim() : "",
                CreatedAt = DateTime.UtcNow 
            });
        }

        return (DocumentType.Unknown, null);
    }
}

// --- DTOs pour Gemini ---

public record GeminiResponse(List<Candidate> Candidates);
public record Candidate(Content Content);
public record Content(List<Part> Parts);
public record Part(string Text);
public record AiResultWrapper(string Type, object Data);
