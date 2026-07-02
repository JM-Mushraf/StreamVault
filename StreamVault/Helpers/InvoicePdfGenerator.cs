using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;

namespace ProjectFileStructure.Helpers
{
    public static class InvoicePdfGenerator
    {
        public static byte[] GenerateInvoice(dynamic receipt)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Grey.Darken3));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text("StreamVault Invoice").FontSize(24).Bold().FontColor(Colors.Red.Medium);
                            column.Item().Text($"Invoice Date: {DateTime.UtcNow:yyyy-MM-dd}");
                            column.Item().Text($"Payment Ref: {receipt.PaymentGuid}");
                        });

                        row.ConstantItem(150).Column(column =>
                        {
                            column.Item().AlignRight().Text("StreamVault Support").Bold();
                            column.Item().AlignRight().Text("townsquare8318@gmail.com");
                            column.Item().AlignRight().Text("https://streamvault.com");
                        });
                    });

                    page.Content().PaddingVertical(25).Column(column =>
                    {
                        column.Spacing(15);

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(billCol =>
                            {
                                billCol.Item().Text("Billed To:").Bold();
                                billCol.Item().Text((string)receipt.FullName);
                                billCol.Item().Text((string)receipt.Email);
                            });
                        });

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Plan Description").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Billing Period").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Amount").Bold();
                            });

                            string planName = receipt.PlanName;
                            string billingPeriod = $"{((DateTime)receipt.StartDate):yyyy-MM-dd} to {((DateTime)receipt.EndDate):yyyy-MM-dd}";
                            decimal price = receipt.Amount;

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(planName);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(billingPeriod);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"${price:N2}");
                        });

                        column.Item().AlignRight().Text($"Total Billed: ${((decimal)receipt.Amount):N2}").FontSize(14).Bold().FontColor(Colors.Red.Medium);
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Thank you for choosing StreamVault! | Page ");
                        x.CurrentPageNumber();
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
