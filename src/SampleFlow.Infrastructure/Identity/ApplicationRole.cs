using Microsoft.AspNetCore.Identity;

namespace SampleFlow.Infrastructure.Identity;

/// <summary>
/// دور النظام (مجموعة صلاحيات جاهزة: Center / Supervisor / Admin).
/// نرث IdentityRole صراحةً لتثبيت نوع المفتاح string وربطه بجدول RolePermissions.
/// </summary>
public class ApplicationRole : IdentityRole
{
    public ApplicationRole() { }

    public ApplicationRole(string roleName) : base(roleName) { }
}
