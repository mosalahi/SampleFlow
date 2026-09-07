namespace SampleFlow.Domain.Entities;

/// <summary>
/// ربط دور جاهز بصلاحية. الأدوار = مجموعات صلاحيات جاهزة (Center/Supervisor/Admin).
/// المفتاح الأساسي مركّب: (RoleId + PermissionId).
/// </summary>
public class RolePermission
{
    /// <summary>معرّف الدور (FK → AspNetRoles).</summary>
    public string RoleId { get; set; } = string.Empty;

    public int PermissionId { get; set; }

    // Navigation
    public Permission Permission { get; set; } = null!;
}
