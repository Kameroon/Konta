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

    public AiParsingService(HttpClient httpClient, IConfiguration configuration, ILogger<AiParsingService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(DocumentType Type, object? Result)> ParseDocumentAsync(string rawText)
    {
        _logger.LogInformation("Lancement du parsing IA (Gemini 1.5 Flash) pour un document de {Length} caractères.", rawText.Length);

        string apiKey = (_configuration["AiSettings:GeminiApiKey"] ?? "").Trim();
        string model = (_configuration["AiSettings:ModelName"] ?? "gemini-1.5-flash").Trim();

        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY" || apiKey == "HIDDEN_IN_USER_SECRETS")
        {
            _logger.LogWarning("Clé API Gemini non configurée ou par défaut. Utilisation du parser de secours (Regex).");
            return ParseWithRegex(rawText);
        }

        try
        {
            var prompt = @"Tu es un expert comptable spécialisé dans l'analyse de documents. 
Analyse le texte suivant extrait d'un PDF et identifie s'il s'agit d'une FACTURE (Invoice) ou d'un RIB (Bank Details).
Retourne EXCLUSIVEMENT un objet JSON structuré comme suit :

Pour une FACTURE:
{ ""type"": ""Invoice"", ""data"": { ""vendor"": ""nom"", ""number"": ""numéro"", ""date"": ""YYYY-MM-DD"", ""totalHt"": 0.0, ""totalTtc"": 0.0, ""vat"": 0.0, ""currency"": ""EUR"" } }

Pour un RIB:
{ ""type"": ""Rib"", ""data"": { ""bank"": ""nom"", ""iban"": ""IBAN sans espaces"", ""bic"": ""BIC"", ""holder"": ""nom du titulaire"" } }

Si tu ne peux pas identifier, retourne: { ""type"": ""Unknown"" }

TEXTE À ANALYSER:
" + rawText;

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
            var response = await _httpClient.PostAsJsonAsync(url, requestBody);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Gemini v1beta non trouvé pour {Model}, tentative avec v1...", model);
                url = $"https://generativelanguage.googleapis.com/v1/models/{model}:generateContent?key={apiKey}";
                response = await _httpClient.PostAsJsonAsync(url, requestBody);
            }

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

            // Nettoyage Markdown si présent
            jsonString = Regex.Replace(jsonString, "```json|```", "").Trim();
            
            var resultDoc = JsonSerializer.Deserialize<AiResultWrapper>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (resultDoc?.Type == "Invoice" && resultDoc.Data is JsonElement invoiceData)
            {
                return (DocumentType.Invoice, new ExtractedInvoice
                {
                    Id = Guid.NewGuid(),
                    VendorName = invoiceData.GetProperty("vendor").GetString() ?? "Inconnu",
                    InvoiceNumber = invoiceData.GetProperty("number").GetString() ?? "Inconnu",
                    InvoiceDate = DateTime.TryParse(invoiceData.GetProperty("date").GetString(), out var d) ? d : DateTime.UtcNow,
                    TotalAmountHt = invoiceData.GetProperty("totalHt").GetDecimal(),
                    TotalAmountTtc = invoiceData.GetProperty("totalTtc").GetDecimal(),
                    VatAmount = invoiceData.GetProperty("vat").GetDecimal(),
                    Currency = invoiceData.GetProperty("currency").GetString() ?? "EUR",
                    RawJson = jsonString
                });
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
            _logger.LogError(ex, "Erreur lors de l'appel à l'API Gemini.");
            return ParseWithRegex(rawText);
        }
    }

    private (DocumentType Type, object? Result) ParseWithRegex(string rawText)
    {
        // ... (Logique Regex existante simplifiée)
        if (rawText.Contains("Facture", StringComparison.OrdinalIgnoreCase))
            return (DocumentType.Invoice, new ExtractedInvoice { Id = Guid.NewGuid(), VendorName = "Regex Match" });
        return (DocumentType.Unknown, null);
    }
}

// --- DTOs pour Gemini ---

public record GeminiResponse(List<Candidate> Candidates);
public record Candidate(Content Content);
public record Content(List<Part> Parts);
public record Part(string Text);
public record AiResultWrapper(string Type, object Data);
