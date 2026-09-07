using ClosedXML.Excel;
using SampleFlow.Web.Models;

namespace SampleFlow.Web.Reports;

/// <summary>يبني ملف Excel للتجميع اليومي من نموذج اللوحة.</summary>
public static class DashboardExcelBuilder
{
    public static byte[] Build(DashboardViewModel model)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("التجميع اليومي");
        ws.RightToLeft = true;

        // عنوان + فترة
        var lastCol = 4 + model.Columns.Count; // المركز، الحالة، المراجعون، [الأنواع]، الإجمالي
        ws.Cell(1, 1).Value = "التجميع اليومي — نظام متابعة العينات";
        ws.Range(1, 1, 1, lastCol).Merge().Style
            .Font.SetBold().Font.SetFontSize(14)
            .Fill.SetBackgroundColor(XLColor.FromHtml("#134e57"))
            .Font.SetFontColor(XLColor.White)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        ws.Cell(2, 1).Value = $"الفترة: {model.Filter.From:yyyy/MM/dd} — {model.Filter.To:yyyy/MM/dd}";
        ws.Range(2, 1, 2, lastCol).Merge();

        // رأس الجدول
        var headerRow = 4;
        var col = 1;
        ws.Cell(headerRow, col++).Value = "المركز";
        ws.Cell(headerRow, col++).Value = "الحالة";
        ws.Cell(headerRow, col++).Value = "المراجعون";
        foreach (var c in model.Columns)
        {
            ws.Cell(headerRow, col++).Value = c.Code;
        }
        ws.Cell(headerRow, col).Value = "الإجمالي";

        var header = ws.Range(headerRow, 1, headerRow, lastCol);
        header.Style.Font.SetBold()
            .Fill.SetBackgroundColor(XLColor.FromHtml("#134e57"))
            .Font.SetFontColor(XLColor.White)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        // صفوف المراكز
        var r = headerRow + 1;
        foreach (var row in model.Rows)
        {
            col = 1;
            ws.Cell(r, col++).Value = row.CenterName;
            ws.Cell(r, col++).Value = row.Entered ? "أُدخلت" : "بانتظار";
            if (row.Entered)
            {
                ws.Cell(r, col++).Value = row.Visitors;
                foreach (var c in model.Columns)
                {
                    ws.Cell(r, col++).Value = row.Counts[c.Id];
                }
                ws.Cell(r, col).Value = row.Total;
            }
            r++;
        }

        // صف الإجمالي
        col = 1;
        ws.Cell(r, col++).Value = $"الإجمالي ({model.CentersEntered} مركز)";
        ws.Cell(r, col++).Value = string.Empty;
        ws.Cell(r, col++).Value = model.TotalVisitors;
        foreach (var c in model.Columns)
        {
            ws.Cell(r, col++).Value = model.ColumnTotals[c.Id];
        }
        ws.Cell(r, col).Value = model.TotalSamples;
        ws.Range(r, 1, r, lastCol).Style.Font.SetBold()
            .Fill.SetBackgroundColor(XLColor.FromHtml("#e8f1f2"));

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
