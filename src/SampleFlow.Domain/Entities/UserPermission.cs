namespace SampleFlow.Domain.Entities;

/// <summary>
/// منح أو سحب صلاحية لمستخدم بعينه خارج دوره.
/// يتجاوز صلاحيات الدور: IsGranted=false يسحب حتى لو منحها الدور، وtrue يمنح حتى لو لم يمنحها.
/// المفتاح الأساسي مركّب: (UserId + PermissionId).
/// </summary>
public class UserPermission
{
    /// <summary>معرّف المستخدم (FK → AspNetUsers).</summary>
    public string UserId { get; set; } = string.Empty;

    public int PermissionId { get; set; }

    /// <summary>true = منح صريح · false = سحب صريح.</summary>
    public bool IsGranted { get; set; }

    // Navigation
    public Permission Permission { get; set; } = null!;
}
