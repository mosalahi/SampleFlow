using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SampleFlow.Domain.Entities;

namespace SampleFlow.Infrastructure.Auditing;

/// <summary>
/// حاوية مؤقتة تُبنى وقت الالتقاط لكل كيان متغيّر، وتتحوّل إلى <see cref="AuditLog"/>
/// بعد الحفظ (لملء المفاتيح المولّدة تلقائياً).
/// </summary>
internal sealed class AuditEntry
{
    public AuditEntry(EntityEntry entry) => Entry = entry;

    public EntityEntry Entry { get; }
    public string EntityName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;

    public Dictionary<string, object?> KeyValues { get; } = new();
    public Dictionary<string, object?> OldValues { get; } = new();
    public Dictionary<string, object?> NewValues { get; } = new();

    /// <summary>خصائص لم تُحسم قيمتها إلا بعد الحفظ (مثل المفتاح المولّد).</summary>
    public List<PropertyEntry> TemporaryProperties { get; } = new();

    public bool HasTemporaryProperties => TemporaryProperties.Count > 0;

    public AuditLog ToAuditLog(IAuditInfoProvider info) => new()
    {
        UserId = info.UserId,
        UserName = info.UserName,
        IpAddress = info.IpAddress,
        Action = Action,
        EntityName = EntityName,
        EntityId = KeyValues.Count > 0
            ? string.Join(",", KeyValues.Values.Select(v => v?.ToString()))
            : null,
        OldValues = OldValues.Count > 0 ? JsonSerializer.Serialize(OldValues) : null,
        NewValues = NewValues.Count > 0 ? JsonSerializer.Serialize(NewValues) : null,
        Timestamp = DateTime.UtcNow,
    };
}
