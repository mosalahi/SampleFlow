namespace SampleFlow.Domain.Authorization;

/// <summary>
/// أسماء الأدوار الجاهزة (مجموعات صلاحيات). الأدوار وسيلة تسهيل فقط،
/// والقرار النهائي مبني على الصلاحيات الدقيقة.
/// </summary>
public static class Roles
{
    public const string Center = "Center";
    public const string Supervisor = "Supervisor";
    public const string Admin = "Admin";

    public static readonly IReadOnlyList<string> All = new[] { Center, Supervisor, Admin };
}
