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
        _logger.LogDebug("Parsing de secours avec Regex...");
        
        bool isInvoice = rawText.Contains("Facture", StringComparison.OrdinalIgnoreCase) 
                      || rawText.Contains("Invoice", StringComparison.OrdinalIgnoreCase)
                      || rawText.Contains("TTC", StringComparison.OrdinalIgnoreCase)
                      || rawText.Contains("Facture N", StringComparison.OrdinalIgnoreCase);

        bool isRib = rawText.Contains("RIB", StringComparison.OrdinalIgnoreCase) 
                  || rawText.Contains("IBAN", StringComparison.OrdinalIgnoreCase)
                  || rawText.Contains("BIC", StringComparison.OrdinalIgnoreCase);

        if (isInvoice)
        {
            // Tentative d'extraction simplifiée
            var invoiceNum = Regex.Match(rawText, @"(?:Facture|Invoice|N°|Numéro)\s*(?:n°|No)?\s*[:\-\s]*([A-Z0-9\-\/]{3,})", RegexOptions.IgnoreCase).Groups[1].Value;
            var dateMatch = Regex.Match(rawText, @"(\d{2}[/\-.]\d{2}[/\-.]\d{4})");
            var amountMatch = Regex.Match(rawText, @"(?:TOTAL|NET|PAYER|REGLER|MONTANT)\s*(?:TTC|À REGLER)?\s*[:]*\s*(\d+[.,\s]*\d{2})", RegexOptions.IgnoreCase);

            // Si le premier pattern échoue pour le montant
            if (!amountMatch.Success) 
                amountMatch = Regex.Match(rawText, @"(\d+[.,\s]*\d{2})\s*(?:€|EUR)", RegexOptions.IgnoreCase);

            DateTime? invoiceDate = null;
            if (dateMatch.Success && DateTime.TryParse(dateMatch.Value.Replace(".", "/"), out var parsedDate))
                invoiceDate = parsedDate;

            decimal totalAmountTtc = 0;
            if (amountMatch.Success)
            {
                var val = amountMatch.Groups[1].Value.Replace(",", ".").Replace(" ", "").Replace("\u00A0", "");
                decimal.TryParse(val, out totalAmountTtc);
            }

            // Détection du vendeur (très basique: première ligne non vide qui n'est pas "Page")
            var lines = rawText.Split(new[] { '\r', '\n', '-' }, StringSplitOptions.RemoveEmptyEntries);
            var vendor = lines.FirstOrDefault(l => !l.Contains("Page", StringComparison.OrdinalIgnoreCase) && l.Length > 3)?.Trim() ?? "Détecté via Regex";

            return (DocumentType.Invoice, new ExtractedInvoice 
            { 
                Id = Guid.NewGuid(), 
                VendorName = vendor,
                InvoiceNumber = !string.IsNullOrWhiteSpace(invoiceNum) ? invoiceNum : "N/A",
                InvoiceDate = invoiceDate ?? DateTime.UtcNow,
                TotalAmountTtc = totalAmountTtc,
                TotalAmountHt = totalAmountTtc / 1.2m, // Estimation 20%
                Currency = "EUR",
                CreatedAt = DateTime.UtcNow,
                RawJson = "{\"source\": \"fallback-regex-advanced\"}"
            });
        }
        
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
