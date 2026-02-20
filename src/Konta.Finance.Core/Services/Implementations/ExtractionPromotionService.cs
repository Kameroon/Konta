using Konta.Finance.Core.Data.Repositories.Interfaces;
using Konta.Finance.Core.Models;
using Konta.Finance.Core.Services.Interfaces;
using Konta.Ocr.Models;
using Microsoft.Extensions.Logging;

namespace Konta.Finance.Core.Services.Implementations;

/// <summary>
/// Service de promotion d'extraction : Transforme les données OCR en objets métiers.
/// Gère l'identification des tiers et la création des factures en brouillon.
/// </summary>
public class ExtractionPromotionService : IExtractionPromotionService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ITierRepository _tierRepository;
    private readonly ILogger<ExtractionPromotionService> _logger;

    public ExtractionPromotionService(
        IInvoiceRepository invoiceRepository,
        ITierRepository tierRepository,
        ILogger<ExtractionPromotionService> logger)
    {
        _invoiceRepository = invoiceRepository;
        _tierRepository = tierRepository;
        _logger = logger;
    }

    public async Task<BusinessInvoice> PromoteToBusinessInvoiceAsync(ExtractedInvoice extractedInvoice, Guid tenantId)
    {
        _logger.LogInformation("Promotion de l'extraction OCR {JobId} pour le tenant {TenantId}", extractedInvoice.JobId, tenantId);

        // 1. Identification ou Création du Tiers (Fournisseur)
        Guid tierId = await GetOrCreateTierIdAsync(extractedInvoice, tenantId);

        // 2. Préparation de la facture métier (BusinessInvoice)
        var businessInvoice = new BusinessInvoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TierId = tierId,
            Reference = extractedInvoice.InvoiceNumber ?? $"OCR-{DateTime.UtcNow:yyyyMMddHHmm}",
            AmountHt = extractedInvoice.TotalAmountHt ?? 0,
            AmountTtc = extractedInvoice.TotalAmountTtc ?? 0,
            VatAmount = extractedInvoice.VatAmount ?? 0,
            InvoiceDate = extractedInvoice.InvoiceDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.UtcNow,
            DueDate = extractedInvoice.DueDate?.ToDateTime(TimeOnly.MinValue) ?? extractedInvoice.InvoiceDate?.ToDateTime(TimeOnly.MinValue).AddDays(30) ?? DateTime.UtcNow.AddDays(30),
            Status = InvoiceStatus.Draft,
            IsPurchase = true, // L'extraction OCR de factures entrantes concerne principalement les achats
            CreatedAt = DateTime.UtcNow
        };

        // 3. Sauvegarde dans le ledger opérationnel
        await _invoiceRepository.CreateAsync(businessInvoice);
        
        _logger.LogInformation("Promotion réussie. Facture créée : {Reference} (ID: {InvoiceId})", businessInvoice.Reference, businessInvoice.Id);
        
        return businessInvoice;
    }

    private async Task<Guid> GetOrCreateTierIdAsync(ExtractedInvoice invoice, Guid tenantId)
    {
        var allTiers = await _tierRepository.GetByTenantIdAsync(tenantId, TierType.Supplier);
        
        // Stratégie 1 : Recherche par SIRET (La plus fiable si extraite)
        if (!string.IsNullOrWhiteSpace(invoice.VendorSiret))
        {
            var matchSiret = allTiers.FirstOrDefault(t => t.Siret == invoice.VendorSiret);
            if (matchSiret != null) return matchSiret.Id;
        }

        // Stratégie 2 : Recherche par Nom (Case-insensitive)
        if (!string.IsNullOrWhiteSpace(invoice.VendorName))
        {
            var matchName = allTiers.FirstOrDefault(t => t.Name.Contains(invoice.VendorName, StringComparison.OrdinalIgnoreCase));
            if (matchName != null) return matchName.Id;
        }

        // Stratégie 3 : Création automatique si non trouvé (Optionnel, dépend de la politique produit)
        _logger.LogInformation("Aucun tiers correspondant trouvé. Création d'un nouveau fournisseur : {VendorName}", invoice.VendorName);
        
        var newTier = new Tier
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = invoice.VendorName ?? "Fournisseur Inconnu (OCR)",
            Siret = invoice.VendorSiret,
            Type = TierType.Supplier,
            CreatedAt = DateTime.UtcNow
        };

        return await _tierRepository.CreateAsync(newTier);
    }
}
