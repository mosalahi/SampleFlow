using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleFlow.Domain.Authorization;

namespace SampleFlow.Web.Controllers;

// شاشة إدخال المركز — تتطلّب صلاحية إضافة إدخال (Center/Admin).
[Authorize(Policy = PermissionKeys.EntriesCreate)]
public class EntryController : Controller
{
    public IActionResult Index() => View();
}
