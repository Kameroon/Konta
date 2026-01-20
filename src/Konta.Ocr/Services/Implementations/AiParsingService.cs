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
        _logger.LogInformation("Lancement du parsing IA pour un document de {Length} caractères.", rawText.Length);

        // Simulation d'un appel LLM (Gemini) avec un prompt système
        // Dans une V2, on utiliserait le SDK Gemini ou un HttpClient vers l'API.
        
        // --- LOGIQUE DU PROMPT (Conceptuelle) ---
        // "Tu es un expert en comptabilité. Analyse le texte suivant et retourne un JSON structuré.
        // Identifie s'il s'agit d'une FACTURE (Invoice) ou d'un RIB.
        // Si FACTURE : { vendor, number, date, totalHt, totalTtc, vat, currency }
        // Si RIB : { bank, iban, bic, holder }
        // Réponds UNIQUEMENT en JSON."

        // Pour la démonstration, on simule une réponse réussie basée sur des patterns si l'API n'est pas configurée
        string apiKey = _configuration["AiSettings:GeminiApiKey"] ?? "";
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY")
        {
             _logger.LogWarning("Clé API Gemini non configurée. Utilisation d'un parser de secours (Regex/Mock).");
             return SimulateAiParsing(rawText);
        }

        // Ici viendrait l'appel réel à l'API LLM...
        return SimulateAiParsing(rawText);
    }

    private (DocumentType Type, object? Result) SimulateAiParsing(string rawText)
    {
        // On cherche des mots clés pour simuler la détection de type
        if (rawText.Contains("Facture", StringComparison.OrdinalIgnoreCase) || rawText.Contains("Invoice", StringComparison.OrdinalIgnoreCase))
        {
            var invoice = new ExtractedInvoice
            {
                Id = Guid.NewGuid(),
                VendorName = "Fournisseur Simulét",
                InvoiceNumber = "FAC-2026-001",
                InvoiceDate = DateTime.UtcNow,
                TotalAmountHt = 100.00m,
                TotalAmountTtc = 120.00m,
                VatAmount = 20.00m,
                Currency = "EUR",
                RawJson = "{ \"status\": \"simulated\" }"
            };
            return (DocumentType.Invoice, invoice);
        }
        else if (rawText.Contains("IBAN", StringComparison.OrdinalIgnoreCase))
        {
            var rib = new ExtractedRib
            {
                Id = Guid.NewGuid(),
                BankName = "Banque de Test",
                Iban = "FR76 1234 5678 9012 3456 7890 123",
                Bic = "TESTFR2P",
                AccountHolder = "M. JEAN TEST"
            };
            return (DocumentType.Rib, rib);
        }

        return (DocumentType.Unknown, null);
    }
}
