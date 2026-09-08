using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SampleFlow.Web.Models;

namespace SampleFlow.Web.Reports;

/// <summary>مستند PDF للتجميع اليومي (RTL، خط Amiri).</summary>
public sealed class DashboardPdfDocument : IDocument
{
    private const string FontName = "Amiri";
    private const string Petrol = "#134e57";
    private const string PetrolSoft = "#e8f1f2";

    private readonly DashboardViewModel _model;

    public DashboardPdfDocument(DashboardViewModel model) => _model = model;

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(28);
            page.DefaultTextStyle(x => x.FontFamily(FontName).FontSize(9));

            page.Header().ContentFromRightToLeft().Column(col =>
            {
                col.Item().Text("التجميع اليومي — نظام متابعة العينات")
                    .FontSize(15).Bold().FontColor(Petrol);
                col.Item().Text($"الفترة: {_model.Filter.From:yyyy/MM/dd} — {_model.Filter.To:yyyy/MM/dd}")
                    .FontSize(9).FontColor(Colors.Grey.Darken1);
                col.Item().PaddingTop(4).Text(
                    $"إجمالي العينات: {_model.TotalSamples}   |   المراجعون: {_model.TotalVisitors}   |   المراكز التي أدخلت: {_model.CentersEntered}/{_model.TotalActiveCenters}")
                    .FontSize(9);
            });

            page.Content().ContentFromRightToLeft().PaddingVertical(8).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2.4f); // المركز
                    columns.RelativeColumn(1.2f); // الحالة
                    columns.RelativeColumn(1.1f); // المراجعون
                    foreach (var _ in _model.Columns)
                    {
                        columns.RelativeColumn();
                    }
                    columns.RelativeColumn(1.1f); // الإجمالي
                });

                table.Header(header =>
                {
                    void H(string text) => header.Cell().Background(Petrol).Padding(4)
                        .Text(text).FontColor(Colors.White).Bold().FontSize(9);

                    H("المركز");
                    H("الحالة");
                    H("المراجعون");
                    foreach (var c in _model.Columns)
                    {
                        H(c.Code);
                    }
                    H("الإجمالي");
                });

                void Cell(string text, bool bold = false)
                {
                    var span = table.Cell()
                        .BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                        .Padding(4).AlignCenter()
                        .Text(text).FontSize(9);
                    if (bold)
                    {
                        span.Bold();
                    }
                }

                void TotalCell(string text) => table.Cell()
                    .Background(PetrolSoft).Padding(4).AlignCenter()
                    .Text(text).Bold().FontSize(9);

                foreach (var row in _model.Rows)
                {
                    Cell(row.CenterName);
                    Cell(row.Entered ? "أُدخلت" : "بانتظار");
                    Cell(row.Entered ? row.Visitors.ToString() : "—");
                    foreach (var c in _model.Columns)
                    {
                        Cell(row.Entered ? row.Counts[c.Id].ToString() : "—");
                    }
                    Cell(row.Entered ? row.Total.ToString() : "—", bold: true);
                }

                TotalCell($"الإجمالي ({_model.CentersEntered} مركز)");
                TotalCell(string.Empty);
                TotalCell(_model.TotalVisitors.ToString());
                foreach (var c in _model.Columns)
                {
                    TotalCell(_model.ColumnTotals[c.Id].ToString());
                }
                TotalCell(_model.TotalSamples.ToString());
            });

            page.Footer().AlignCenter()
                .Text($"SampleFlow — طُبع في {DateTime.Now:yyyy/MM/dd HH:mm}")
                .FontSize(8).FontColor(Colors.Grey.Medium);
        });
    }
}
