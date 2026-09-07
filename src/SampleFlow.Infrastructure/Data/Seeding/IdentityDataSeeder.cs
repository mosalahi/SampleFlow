using Microsoft.AspNetCore.Identity;
using SampleFlow.Domain.Authorization;
using SampleFlow.Infrastructure.Identity;

namespace SampleFlow.Infrastructure.Data.Seeding;

/// <summary>
/// بذر بيانات Identity وقت التشغيل: يضمن وجود الأدوار الثلاثة، ثم ينشئ مستخدم
/// أدمن أولياً (تجزئة كلمة المرور عبر UserManager) ويُلحقه بدور Admin.
/// إجراء idempotent — آمن التكرار عند كل إقلاع.
/// يُستدعى من مضيف الويب في مرحلة لاحقة بعد تطبيق الـ Migrations.
/// </summary>
public class IdentityDataSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public IdentityDataSeeder(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>
    /// يضمن وجود الأدوار ومستخدم الأدمن. تُمرَّر بيانات الأدمن من الإعدادات
    /// (متغيرات بيئة/أسرار) لتفادي أي سرّ ثابت في الكود.
    /// </summary>
    public async Task SeedAsync(string adminEmail, string adminPassword)
    {
        await EnsureRolesAsync();
        await EnsureAdminUserAsync(adminEmail, adminPassword);
    }

    private async Task EnsureRolesAsync()
    {
        foreach (var roleName in Roles.All)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new ApplicationRole(roleName));
            }
        }
    }

    private async Task EnsureAdminUserAsync(string adminEmail, string adminPassword)
    {
        var admin = await _userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                CenterId = null, // الأدمن غير مرتبط بمركز.
            };

            var result = await _userManager.CreateAsync(admin, adminPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new InvalidOperationException($"تعذّر إنشاء مستخدم الأدمن: {errors}");
            }
        }

        if (!await _userManager.IsInRoleAsync(admin, Roles.Admin))
        {
            await _userManager.AddToRoleAsync(admin, Roles.Admin);
        }
    }
}
