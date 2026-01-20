using Dapper;
using Konta.Finance.Core.Data.Repositories.Interfaces;
using Konta.Finance.Core.Models;
using Konta.Shared.Data;
using Konta.Shared.Data.Repositories;

namespace Konta.Finance.Core.Data.Repositories.Implementations;

/// <summary>
/// Gestion de la persistance des factures opérationnelles.
/// </summary>
public class InvoiceRepository : BaseRepository<InvoiceRepository>, IInvoiceRepository
{
    public InvoiceRepository(IDbConnectionFactory connectionFactory, ILogger<InvoiceRepository> logger) 
        : base(connectionFactory, logger)
    {
    }

    /// <summary>
    /// Récupère une facture par son ID.
    /// </summary>
    public async Task<BusinessInvoice?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT * FROM finance_core.BusinessInvoices WHERE Id = @Id";
        
        _logger.LogDebug("Recherche facture par ID : {Id}", id);
        
        using var connection = CreateConnection(sql, new { Id = id });
        return await connection.QuerySingleOrDefaultAsync<BusinessInvoice>(sql, new { Id = id });
    }

    /// <summary>
    /// Liste les factures d'un tenant avec filtres optionnels.
    /// </summary>
    public async Task<IEnumerable<BusinessInvoice>> GetByTenantIdAsync(Guid tenantId, bool? isPurchase = null, InvoiceStatus? status = null)
    {
        var sql = "SELECT * FROM finance_core.BusinessInvoices WHERE TenantId = @TenantId";
        if (isPurchase.HasValue) sql += " AND IsPurchase = @IsPurchase";
        if (status.HasValue) sql += " AND Status = @Status";
        sql += " ORDER BY InvoiceDate DESC";

        _logger.LogDebug("Listing factures pour Tenant : {TenantId} (Achat: {IsPurchase}, Statut: {Status})", tenantId, isPurchase, status);

        using var connection = CreateConnection(sql, new { TenantId = tenantId, IsPurchase = isPurchase, Status = status });
        return await connection.QueryAsync<BusinessInvoice>(sql, new { TenantId = tenantId, IsPurchase = isPurchase, Status = status });
    }

    /// <summary>
    /// Crée une nouvelle facture opérationnelle.
    /// </summary>
    public async Task<Guid> CreateAsync(BusinessInvoice invoice)
    {
        const string sql = @"
            INSERT INTO finance_core.BusinessInvoices (Id, TenantId, TierId, Reference, AmountHt, AmountTtc, VatAmount, InvoiceDate, DueDate, Status, IsPurchase, CreatedAt)
            VALUES (@Id, @TenantId, @TierId, @Reference, @AmountHt, @AmountTtc, @VatAmount, @InvoiceDate, @DueDate, @Status, @IsPurchase, @CreatedAt)
            RETURNING Id";
            
        _logger.LogInformation("Enregistrement d'une nouvelle facture : {Ref} (Montant: {Amount})", invoice.Reference, invoice.AmountTtc);
        
        using var connection = CreateConnection(sql, invoice);
        return await connection.ExecuteScalarAsync<Guid>(sql, invoice);
    }

    /// <summary>
    /// Met à jour le statut d'une facture.
    /// </summary>
    public async Task<bool> UpdateStatusAsync(Guid id, InvoiceStatus status)
    {
        const string sql = "UPDATE finance_core.BusinessInvoices SET Status = @Status, UpdatedAt = @UpdatedAt WHERE Id = @Id";
        
        _logger.LogInformation("Changement de statut pour la facture {Id} -> {Status}", id, status);
        
        var parameters = new { Id = id, Status = status, UpdatedAt = DateTime.UtcNow };
        using var connection = CreateConnection(sql, parameters);
        var rows = await connection.ExecuteAsync(sql, parameters);
        return rows > 0;
    }
}
