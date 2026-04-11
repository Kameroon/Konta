using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Konta.Billing.Models;
using Konta.Billing.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Konta.Billing.Services.Implementations;

/// <summary>
/// Service de génération de factures PDF haute performance.
/// </summary>
public class InvoiceService : IInvoiceService
{
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(ILogger<InvoiceService> logger)
    {
        _logger = logger;
        
        // QuestPDF License configuration (Free for small businesses/dev)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <inheritdoc />
    public byte[] GenerateInvoicePdf(BillingInvoice invoice, string tenantName)
    {
        _logger.LogInformation("Génération du PDF pour la facture {Number}", invoice.InvoiceNumber);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Helvetica"));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("KONTA ERP").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                        col.Item().Text("Facture de Service SaaS");
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text($"Facture : {invoice.InvoiceNumber}").SemiBold();
                        col.Item().Text($"Date : {invoice.CreatedAt:dd/MM/yyyy}");
                    });
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                {
                    x.Spacing(20);

                    x.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Client").SemiBold();
                            c.Item().Text(tenantName);
                        });
                    });

                    x.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Description");
                            header.Cell().Element(CellStyle).AlignRight().Text("Quantité");
                            header.Cell().Element(CellStyle).AlignRight().Text("Total");

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                            }
                        });

                        table.Cell().Element(ValueStyle).Text("Abonnement Konta ERP - Plan Mensuel");
                        table.Cell().Element(ValueStyle).AlignRight().Text("1");
                        table.Cell().Element(ValueStyle).AlignRight().Text($"{invoice.Amount:N2} {invoice.Currency}");

                        static IContainer ValueStyle(IContainer container)
                        {
                            return container.PaddingVertical(5);
                        }
                    });

                    x.Item().AlignRight().Text($"TOTAL : {invoice.Amount:N2} {invoice.Currency}").FontSize(14).SemiBold();
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                });
            });
        });

        return document.GeneratePdf();
    }
}
