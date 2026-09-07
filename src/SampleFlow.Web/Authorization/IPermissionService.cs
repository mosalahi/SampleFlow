namespace SampleFlow.Web.Authorization;

/// <summary>
/// يحسب الصلاحيات الفعّالة للمستخدم من قاعدة البيانات (صلاحيات أدواره ± استثناءاته)،
/// فتُطبّق أي تعديلات فوراً دون انتظار إعادة تسجيل الدخول.
/// </summary>
public interface IPermissionService
{
    Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(string userId, CancellationToken cancellationToken = default);

    Task<bool> HasPermissionAsync(string userId, string permissionKey, CancellationToken cancellationToken = default);
}
