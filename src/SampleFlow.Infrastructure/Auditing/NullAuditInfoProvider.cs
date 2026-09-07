namespace SampleFlow.Infrastructure.Auditing;

/// <summary>
/// تنفيذ افتراضي بلا سياق مستخدم (تُسجَّل الأحداث بمستخدم/IP فارغ).
/// يُستخدم حين لا يوجد طلب ويب.
/// </summary>
public sealed class NullAuditInfoProvider : IAuditInfoProvider
{
    public static readonly NullAuditInfoProvider Instance = new();

    public string? UserId => null;
    public string? UserName => null;
    public string? IpAddress => null;
}
