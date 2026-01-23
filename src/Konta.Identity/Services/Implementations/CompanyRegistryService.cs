using System.Net.Http.Json;
using System.Text.Json;
using Konta.Identity.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Konta.Identity.Services.Implementations;

public class CompanyRegistryService : ICompanyRegistryService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CompanyRegistryService> _logger;

    public CompanyRegistryService(HttpClient httpClient, ILogger<CompanyRegistryService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CompanyRegistrationResult?> LookupBySiretAsync(string siret)
    {
        if (string.IsNullOrWhiteSpace(siret) || siret.Length < 9) return null;

        _logger.LogInformation("Recherche d'entreprise pour le SIRET/SIREN : {Siret}", siret);

        try
        {
            // L'API de recherche accepte SIREN ou SIRET dans le paramètre 'q'
            var url = $"https://recherche-entreprises.api.gouv.fr/search?q={siret}";
            
            // Ajout d'un User-Agent car certaines APIs gouv l'exigent ou le préfèrent
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("KontaERP/1.0 (https://konta.fr)");

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("L'API de recherche d'entreprises a retourné un statut {StatusCode}", response.StatusCode);
                return null;
            }

            var root = await response.Content.ReadFromJsonAsync<JsonElement>();
            
            if (root.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array && results.GetArrayLength() > 0)
            {
                var first = results[0];
                
                string name = first.GetProperty("nom_complet").GetString() ?? "";
                string industry = first.GetProperty("activite_principale").GetString() ?? "";
                
                string address = "";
                if (first.TryGetProperty("siege", out var siege))
                {
                    address = siege.GetProperty("adresse").GetString() ?? "";
                }

                return new CompanyRegistrationResult
                {
                    Name = name,
                    Address = address,
                    Industry = industry,
                    CommercialIdentifier = siret
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'appel à l'API de recherche d'entreprises pour le SIRET {Siret}", siret);
            return null;
        }
    }
}
