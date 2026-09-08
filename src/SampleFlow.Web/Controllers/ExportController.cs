using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using SampleFlow.Domain.Authorization;
using SampleFlow.Web.Reports;
using SampleFlow.Web.Services;

namespace SampleFlow.Web.Controllers;

// التصدير — يتطلّب صلاحية تصدير التقارير (Supervisor/Admin).
[Authorize(Policy = PermissionKeys.ReportsExport)]
public class ExportController : Controller
{
    private const string ExcelContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly IDashboardService _dashboard;

    public ExportController(IDashboardService dashboard) => _dashboard = dashboard;

    [HttpGet]
    public async Task<IActionResult> Excel(DateOnly? from, DateOnly? to, int? centerId, int? sampleTypeId)
    {
        var vm = await _dashboard.BuildAsync(from, to, centerId, sampleTypeId);
        var bytes = DashboardExcelBuilder.Build(vm);
        return File(bytes, ExcelContentType, FileName(vm.Filter.From, vm.Filter.To, "xlsx"));
    }

    [HttpGet]
    public async Task<IActionResult> Pdf(DateOnly? from, DateOnly? to, int? centerId, int? sampleTypeId)
    {
        var vm = await _dashboard.BuildAsync(from, to, centerId, sampleTypeId);
        var bytes = new DashboardPdfDocument(vm).GeneratePdf();
        return File(bytes, "application/pdf", FileName(vm.Filter.From, vm.Filter.To, "pdf"));
    }

    private static string FileName(DateOnly from, DateOnly to, string ext)
        => $"SampleFlow_{from:yyyyMMdd}_{to:yyyyMMdd}.{ext}";
}
