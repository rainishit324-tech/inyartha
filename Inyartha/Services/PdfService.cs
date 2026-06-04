using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using InyarthaApp.Models;

namespace InyarthaApp.Services;

public class PdfService
{
    private readonly string _pdfFolder;
    private readonly string _wwwroot;

    private static readonly Color Gold = Color.FromHex("#b89968");
    private static readonly Color GoldDeep = Color.FromHex("#8a6d3f");
    private static readonly Color Ink = Color.FromHex("#1a1410");
    private static readonly Color InkSoft = Color.FromHex("#5c4f43");
    private static readonly Color CreamBg = Color.FromHex("#faf7f2");

    public PdfService(IWebHostEnvironment env)
    {
        _pdfFolder = Path.Combine(env.ContentRootPath, "Data", "Quotes");
        _wwwroot = Path.Combine(env.ContentRootPath, "wwwroot");
        Directory.CreateDirectory(_pdfFolder);
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public string GetPdfPath(string id) => Path.Combine(_pdfFolder, $"{id}.pdf");
    public bool Exists(string id) => File.Exists(GetPdfPath(id));

    public async Task GenerateAsync(Quote quote)
    {
        var path = GetPdfPath(quote.Id);
        var logoPath = Path.Combine(_wwwroot, "images", "logo.png");
        var sigPath = Path.Combine(_wwwroot, "images", "signature.png");

        if (File.Exists(path))
            File.Delete(path);

        await Task.Run(() =>
        {
            Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.MarginLeft(40);
                    page.MarginRight(40);
                    page.MarginTop(50);
                    page.MarginBottom(50);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10).FontColor(Ink));

                    page.Header().Element(c => ComposeHeader(c, quote, logoPath));
                    page.Content().Element(c => ComposeContent(c, quote, sigPath));
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf(path);
        });
    }

    public async Task DeleteAsync(string id)
    {
        var path = GetPdfPath(id);
        if (File.Exists(path))
        {
            await Task.Run(() => File.Delete(path));
        }
    }

    private void ComposeHeader(IContainer container, Quote quote, string logoPath)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().AlignCenter().Column(c =>
                {
                    if (File.Exists(logoPath))
                        c.Item().Height(50).Image(logoPath);
                });
            });
            col.Item().AlignCenter().PaddingTop(4).Text("Design Studio")
                .FontSize(10).FontColor(InkSoft);
            col.Item().PaddingTop(6).LineHorizontal(1).LineColor(GoldDeep);
            col.Item().PaddingTop(12).Row(row =>
            {
                row.RelativeItem().Text($"Quote #: {quote.Id}")
                    .FontSize(11).SemiBold().FontColor(Ink);
                row.RelativeItem().AlignRight().Text($"Date: {quote.CreatedAt:dd MMM yyyy}")
                    .FontSize(11).FontColor(InkSoft);
            });
        });
    }

    private void ComposeContent(IContainer container, Quote quote, string sigPath)
    {
        container.Column(col =>
        {
            SectionTitle(col, "CLIENT DETAILS");
            col.Item().PaddingLeft(4).Column(c =>
            {
                DetailRow(c, "Name", quote.ClientName);
                DetailRow(c, "Email", quote.ClientEmail);
                DetailRow(c, "Phone", string.IsNullOrEmpty(quote.ClientPhone) ? "\u2014" : quote.ClientPhone);
            });

            SectionTitle(col, "PROJECT DETAILS");
            col.Item().PaddingLeft(4).Column(c =>
            {
                DetailRow(c, "Type", quote.ProjectType);
                DetailRow(c, "Area", quote.AreaSqFt.HasValue ? $"{quote.AreaSqFt} sq ft" : "\u2014");
                DetailRow(c, "Description", quote.Scope ?? "\u2014");
            });

            if (quote.LineItems?.Count > 0)
            {
                SectionTitle(col, "LINE ITEMS");
                ComposeLineItemsTable(col, quote.LineItems);
            }

            col.Item().PaddingTop(24).AlignRight().Column(c =>
            {
                if (File.Exists(sigPath))
                    c.Item().Height(50).Image(sigPath);
                c.Item().PaddingTop(2).AlignRight().Text("(Authorised Signatory)")
                    .FontSize(8).Italic().FontColor(InkSoft);
            });
        });
    }

    private static void SectionTitle(ColumnDescriptor col, string title)
    {
        col.Item().PaddingTop(16).Text(title)
            .FontSize(12).Bold().FontColor(GoldDeep);
        col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Gold);
    }

    private static void DetailRow(ColumnDescriptor col, string label, string? value)
    {
        col.Item().PaddingVertical(1).Text(t =>
        {
            t.Span($"{label}: ").SemiBold().FontSize(10);
            t.Span(value ?? "\u2014").FontSize(10).FontColor(InkSoft);
        });
    }

    private static void ComposeLineItemsTable(ColumnDescriptor col, List<QuoteLineItem> items)
    {
        var grandTotal = items.Sum(i => i.Total);

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(3);
                c.RelativeColumn(2);
                c.RelativeColumn(1);
                c.RelativeColumn(1);
                c.RelativeColumn(1.5f);
            });

            table.Header(header =>
            {
                header.Cell().PaddingVertical(6).PaddingHorizontal(8)
                    .Background(GoldDeep)
                    .DefaultTextStyle(x => x.FontSize(9).Bold().FontColor(Colors.White))
                    .Text("Item");
                header.Cell().PaddingVertical(6).PaddingHorizontal(8)
                    .Background(GoldDeep)
                    .DefaultTextStyle(x => x.FontSize(9).Bold().FontColor(Colors.White))
                    .Text("Material");
                header.Cell().PaddingVertical(6).PaddingHorizontal(8).AlignRight()
                    .Background(GoldDeep)
                    .DefaultTextStyle(x => x.FontSize(9).Bold().FontColor(Colors.White))
                    .Text("Area");
                header.Cell().PaddingVertical(6).PaddingHorizontal(8).AlignRight()
                    .Background(GoldDeep)
                    .DefaultTextStyle(x => x.FontSize(9).Bold().FontColor(Colors.White))
                    .Text("Rate");
                header.Cell().PaddingVertical(6).PaddingHorizontal(8).AlignRight()
                    .Background(GoldDeep)
                    .DefaultTextStyle(x => x.FontSize(9).Bold().FontColor(Colors.White))
                    .Text("Total");
            });

            for (int i = 0; i < items.Count; i++)
            {
                var li = items[i];
                var bg = i % 2 == 0 ? Colors.White : CreamBg;

                table.Cell().PaddingVertical(4).PaddingHorizontal(8).Background(bg)
                    .Text(t =>
                    {
                        t.Span(li.ItemName).FontSize(9);
                        if (!string.IsNullOrEmpty(li.Description))
                        {
                            t.Span("\n" + li.Description).FontSize(8).FontColor(InkSoft);
                        }
                    });
                table.Cell().PaddingVertical(4).PaddingHorizontal(8).Background(bg)
                    .Text(string.IsNullOrEmpty(li.Material) ? "\u2014" : li.Material)
                    .FontSize(9).FontColor(InkSoft);
                table.Cell().PaddingVertical(4).PaddingHorizontal(8).Background(bg).AlignRight()
                    .Text($"{li.Area:N2}").FontSize(9);
                table.Cell().PaddingVertical(4).PaddingHorizontal(8).Background(bg).AlignRight()
                    .Text($"Rs. {li.UnitPrice:N2}").FontSize(9);
                table.Cell().PaddingVertical(4).PaddingHorizontal(8).Background(bg).AlignRight()
                    .Text($"Rs. {li.Total:N2}").FontSize(9).SemiBold().FontColor(GoldDeep);
            }

            table.Footer(footer =>
            {
                footer.Cell().ColumnSpan(4).PaddingVertical(6).PaddingHorizontal(8)
                    .DefaultTextStyle(x => x.FontSize(10).Bold())
                    .Text("Grand Total");
                footer.Cell().PaddingVertical(6).PaddingHorizontal(8).AlignRight()
                    .DefaultTextStyle(x => x.FontSize(10).Bold().FontColor(GoldDeep))
                    .Text($"Rs. {grandTotal:N2}");
            });
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Column(c =>
        {
            c.Item().LineHorizontal(1).LineColor(Gold);
            c.Item().PaddingTop(4).Text("Inyartha Design Studio | Budigere Cross, Bengaluru 560049")
                .FontSize(8).FontColor(InkSoft);
            c.Item().Text("+91 81232 97276 | hello@inyartha.com")
                .FontSize(8).FontColor(InkSoft);
        });
    }
}
