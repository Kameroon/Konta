using Konta.Finance.Core.Models;
using Konta.Ocr.Models;

namespace Konta.Finance.Core.Services.Interfaces;

/// <summary>
/// Service responsable de la conversion (promotion) des résultats OCR 
/// en entités métiers (BusinessInvoice, etc.) au sein du module Finance.
/// </summary>
public interface IExtractionPromotionService
{
    /// <summary>
    /// Crée une facture d'achat brouillon à partir d'un résultat d'extraction.
    /// Vérifie la légitimité du tenant et l'identification du tiers via SIRET.
    /// </summary>
    /// <param name="extractedInvoice">Données issues de l'OCR.</param>
    /// <param name="tenantId">ID du tenant propriétaire.</param>
    /// <returns>La facture métier créée.</returns>
    Task<BusinessInvoice> PromoteToBusinessInvoiceAsync(ExtractedInvoice extractedInvoice, Guid tenantId);
}
