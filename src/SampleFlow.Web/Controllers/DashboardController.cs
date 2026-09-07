using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleFlow.Domain.Authorization;

namespace SampleFlow.Web.Controllers;

// لوحة المشرف — تتطلّب صلاحية عرض جميع الإدخالات (Supervisor/Admin).
[Authorize(Policy = PermissionKeys.EntriesViewAny)]
public class DashboardController : Controller
{
    public IActionResult Index() => View();
}
