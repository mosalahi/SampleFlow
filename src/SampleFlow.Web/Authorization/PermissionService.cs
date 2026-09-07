using Microsoft.EntityFrameworkCore;
using SampleFlow.Infrastructure.Data;

namespace SampleFlow.Web.Authorization;

/// <summary>
/// تحسب الصلاحيات الفعّالة: اتحاد صلاحيات أدوار المستخدم، ثم تطبيق استثناءات
/// المستخدم (IsGranted=true يُضيف، false يسحب). القراءة من القاعدة في كل طلب،
/// مع حفظ مؤقت ضمن نطاق الطلب الواحد لتفادي تكرار الاستعلام.
/// </summary>
public sealed class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _db;
    private readonly Dictionary<string, IReadOnlySet<string>> _requestCache = new();

    public PermissionService(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        if (_requestCache.TryGetValue(userId, out var cached))
        {
            return cached;
        }

        var roleIds = await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        var rolePermissionKeys = await _db.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.Key)
            .ToListAsync(cancellationToken);

        var userOverrides = await _db.UserPermissions
            .Where(up => up.UserId == userId)
            .Select(up => new { up.Permission.Key, up.IsGranted })
            .ToListAsync(cancellationToken);

        var effective = new HashSet<string>(rolePermissionKeys, StringComparer.Ordinal);
        foreach (var o in userOverrides)
        {
            if (o.IsGranted)
            {
                effective.Add(o.Key);
            }
            else
            {
                effective.Remove(o.Key);
            }
        }

        _requestCache[userId] = effective;
        return effective;
    }

    public async Task<bool> HasPermissionAsync(
        string userId, string permissionKey, CancellationToken cancellationToken = default)
        => (await GetEffectivePermissionsAsync(userId, cancellationToken)).Contains(permissionKey);
}
