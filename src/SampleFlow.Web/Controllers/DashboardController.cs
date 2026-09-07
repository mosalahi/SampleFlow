using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleFlow.Domain.Authorization;
using SampleFlow.Web.Services;

namespace SampleFlow.Web.Controllers;

// لوحة المشرف — تتطلّب صلاحية عرض جميع الإدخالات (Supervisor/Admin).
[Authorize(Policy = PermissionKeys.EntriesViewAny)]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard) => _dashboard = dashboard;

    public async Task<IActionResult> Index(DateOnly? from, DateOnly? to, int? centerId, int? sampleTypeId)
    {
        var vm = await _dashboard.BuildAsync(from, to, centerId, sampleTypeId);
        return View(vm);
    }
}
