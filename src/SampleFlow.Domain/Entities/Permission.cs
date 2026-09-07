namespace SampleFlow.Domain.Entities;

/// <summary>
/// صلاحية دقيقة مخزّنة في قاعدة البيانات (مثل Entries.EditAny).
/// النظام مبني على الصلاحيات لا الأدوار الثابتة، وتُطبّق عبر Policies ديناميكية.
/// </summary>
public class Permission
{
    public int Id { get; set; }

    /// <summary>المفتاح الفريد المستخدم في السياسات (مثل "Entries.EditAny").</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>الاسم المعروض في واجهة إدارة الصلاحيات.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>تصنيف الصلاحية للتجميع في الواجهة (مثل "Entries", "Reports").</summary>
    public string Category { get; set; } = string.Empty;

    // Navigation
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
