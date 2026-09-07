using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SampleFlow.Domain.Authorization;
using SampleFlow.Domain.Entities;
using SampleFlow.Infrastructure.Data;
using SampleFlow.Infrastructure.Identity;
using SampleFlow.Web.Models;

namespace SampleFlow.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _db = db;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByNameAsync(model.UserName)
                   ?? await _userManager.FindByEmailAsync(model.UserName);

        if (user is not null)
        {
            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                await LogLoginAsync(user);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return await RedirectByRoleAsync(user);
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "الحساب مقفل مؤقتاً بسبب محاولات دخول متكرّرة. حاول لاحقاً.");
                return View(model);
            }
        }

        ModelState.AddModelError(string.Empty, "بيانات الدخول غير صحيحة.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    private async Task<IActionResult> RedirectByRoleAsync(ApplicationUser user)
    {
        // Supervisor/Admin ← لوحة المشرف، Center ← شاشة الإدخال.
        if (await _userManager.IsInRoleAsync(user, Roles.Admin)
            || await _userManager.IsInRoleAsync(user, Roles.Supervisor))
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return RedirectToAction("Index", "Entry");
    }

    private async Task LogLoginAsync(ApplicationUser user)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            UserName = user.UserName,
            Action = "Login",
            EntityName = "AspNetUsers",
            EntityId = user.Id,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Timestamp = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync();
    }
}
