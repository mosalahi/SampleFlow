using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SampleFlow.Web.Models;

public sealed class UserListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Roles { get; set; } = string.Empty;
    public string? CenterName { get; set; }
}

public sealed class UserCreateViewModel
{
    [Required(ErrorMessage = "أدخل البريد")]
    [EmailAddress(ErrorMessage = "بريد غير صالح")]
    [Display(Name = "البريد الإلكتروني")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "أدخل كلمة المرور")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "كلمة المرور 8 أحرف على الأقل")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "اختر الدور")]
    [Display(Name = "الدور")]
    public string Role { get; set; } = string.Empty;

    [Display(Name = "المركز")]
    public int? CenterId { get; set; }

    public List<SelectListItem> RoleOptions { get; set; } = new();
    public List<SelectListItem> CenterOptions { get; set; } = new();
}

public sealed class UserEditViewModel
{
    public string Id { get; set; } = string.Empty;

    [Display(Name = "البريد الإلكتروني")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "اختر الدور")]
    [Display(Name = "الدور")]
    public string Role { get; set; } = string.Empty;

    [Display(Name = "المركز")]
    public int? CenterId { get; set; }

    public List<SelectListItem> RoleOptions { get; set; } = new();
    public List<SelectListItem> CenterOptions { get; set; } = new();
}

public sealed class UserPermissionsViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<PermissionRowInput> Rows { get; set; } = new();
}

public sealed class PermissionRowInput
{
    public int PermissionId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    /// <summary>هل يمنحها دور المستخدم (للعرض فقط).</summary>
    public bool RoleGrants { get; set; }

    /// <summary>inherit | grant | revoke</summary>
    public string State { get; set; } = "inherit";
}
