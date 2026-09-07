using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SampleFlow.Web.Models;

namespace SampleFlow.Web.Reports;

/// <summary>مستند PDF للتجميع اليومي (RTL، خط Amiri).</summary>
public sealed class DashboardPdfDocument : IDocument
{
    private const string FontFamily = "Amiri";
    private static readonly string Petrol = "#134e57";
    private static readonly string PetrolSoft = "#e8f1f2";

    private readonly DashboardViewModel _model;

    public DashboardPdfDocument(DashboardViewModel model) => _model = model;

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(28);
            page.DefaultTextStyle(x => x.FontFamily(FontFamily).FontSize(9));
            page.ContentFromRightToLeft();

            page.Header().Column(header =>
            {
                header.Item().Text("التجميع اليومي — نظام متابعة العينات")
                    .FontSize(15).Bold().FontColor(Petrol);
                header.Item().Text($"الفترة: {_model.Filter.From:yyyy/MM/dd} — {_model.Filter.To:yyyy/MM/dd}")
                    .FontSize(9).FontColor(Colors.Grey.Darken1);
                header.Item().PaddingTop(4).Text(
                    $"إجمالي العينات: {_model.TotalSamples}    |    المراجعون: {_model.TotalVisitors}    |    المراكز التي أدخلت: {_model.CentersEntered}/{_model.TotalActiveCenters}")
                    .FontSize(9);
            });

            page.Content().PaddingVertical(8).Table(table =>
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

                table.Header(h =>
                {
                    HeaderCell(h, "المركز");
                    HeaderCell(h, "الحالة");
                    HeaderCell(h, "المراجعون");
                    foreach (var c in _model.Columns)
                    {
                        HeaderCell(h, c.Code);
                    }
                    HeaderCell(h, "الإجمالي");
                });

                foreach (var row in _model.Rows)
                {
                    BodyCell(table, row.CenterName, alignStart: true);
                    BodyCell(table, row.Entered ? "أُدخلت" : "بانتظار");
                    BodyCell(table, row.Entered ? row.Visitors.ToString() : "—");
                    foreach (var c in _model.Columns)
                    {
                        BodyCell(table, row.Entered ? row.Counts[c.Id].ToString() : "—");
                    }
                    BodyCell(table, row.Entered ? row.Total.ToString() : "—", bold: true);
                }

                // صف الإجمالي
                TotalCell(table, $"الإجمالي ({_model.CentersEntered} مركز)", alignStart: true);
                TotalCell(table, string.Empty);
                TotalCell(table, _model.TotalVisitors.ToString());
                foreach (var c in _model.Columns)
                {
                    TotalCell(table, _model.ColumnTotals[c.Id].ToString());
                }
                TotalCell(table, _model.TotalSamples.ToString());
            });

            page.Footer().AlignCenter().Text(x =>
            {
                x.Span("SampleFlow — ");
                x.Span($"طُبع في {DateTime.Now:yyyy/MM/dd HH:mm}").FontColor(Colors.Grey.Medium);
            });
        });
    }

    private static void HeaderCell(TableCellDescriptor descriptor, string text) =>
        descriptor.Cell().Background(Petrol).Padding(4)
            .Text(text).FontColor(Colors.White).Bold().FontSize(9);

    private static void BodyCell(TableDescriptor table, string text, bool bold = false, bool alignStart = false)
    {
        var cell = table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4);
        var span = (alignStart ? cell.AlignRight() : cell.AlignCenter()).Text(text).FontSize(9);
        if (bold)
        {
            span.Bold();
        }
    }

    private static void TotalCell(TableDescriptor table, string text, bool alignStart = false)
    {
        var cell = table.Cell().Background(PetrolSoft).Padding(4);
        (alignStart ? cell.AlignRight() : cell.AlignCenter()).Text(text).Bold().FontSize(9);
    }
}
