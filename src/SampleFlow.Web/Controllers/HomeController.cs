using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleFlow.Domain.Authorization;

namespace SampleFlow.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    // نقطة الدخول: توجيه حسب الدور.
    public IActionResult Index()
    {
        if (User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Supervisor))
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return RedirectToAction("Index", "Entry");
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
