using Konta.Billing.Models;

namespace Konta.Billing.Services.Interfaces;

public interface IInvoiceService
{
    byte[] GenerateInvoicePdf(BillingInvoice invoice, string tenantName);
}
