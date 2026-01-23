using System.Text.Json.Serialization;

namespace Konta.Identity.Services.Interfaces;

public record CompanyRegistrationResult
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("address")]
    public string? Address { get; init; }

    [JsonPropertyName("industry")]
    public string? Industry { get; init; }

    [JsonPropertyName("commercialIdentifier")]
    public string? CommercialIdentifier { get; init; }
}

public interface ICompanyRegistryService
{
    Task<CompanyRegistrationResult?> LookupBySiretAsync(string siret);
}
