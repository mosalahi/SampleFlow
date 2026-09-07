namespace SampleFlow.Domain.Authorization;

/// <summary>
/// مفاتيح الصلاحيات الدقيقة. تُستخدم كأسماء سياسات في التفويض الديناميكي:
/// [Authorize(Policy = PermissionKeys.EntriesEditAny)].
/// </summary>
public static class PermissionKeys
{
    public const string EntriesCreate = "Entries.Create";
    public const string EntriesEditOwn = "Entries.EditOwn";
    public const string EntriesEditAny = "Entries.EditAny";
    public const string EntriesViewOwn = "Entries.ViewOwn";
    public const string EntriesViewAny = "Entries.ViewAny";
    public const string ReportsView = "Reports.View";
    public const string ReportsExport = "Reports.Export";
    public const string CentersManage = "Centers.Manage";
    public const string SampleTypesManage = "SampleTypes.Manage";
    public const string UsersManage = "Users.Manage";
    public const string AuditView = "Audit.View";

    /// <summary>كل المفاتيح، بترتيب ثابت (مفيد للبذر والتحقق).</summary>
    public static readonly IReadOnlyList<string> All = new[]
    {
        EntriesCreate,
        EntriesEditOwn,
        EntriesEditAny,
        EntriesViewOwn,
        EntriesViewAny,
        ReportsView,
        ReportsExport,
        CentersManage,
        SampleTypesManage,
        UsersManage,
        AuditView,
    };
}
