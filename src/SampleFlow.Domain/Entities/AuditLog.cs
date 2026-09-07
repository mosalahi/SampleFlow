namespace SampleFlow.Domain.Entities;

/// <summary>
/// سجل تتبّع مركزي. يُملأ تلقائياً عبر اعتراض SaveChanges في الـ DbContext (المرحلة 3).
/// </summary>
public class AuditLog
{
    public long Id { get; set; }

    /// <summary>معرّف المستخدم الذي قام بالإجراء (قد يكون null لأحداث النظام).</summary>
    public string? UserId { get; set; }

    /// <summary>لقطة اسم المستخدم وقت الحدث.</summary>
    public string? UserName { get; set; }

    /// <summary>نوع الإجراء: Create / Update / Delete / Login.</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>اسم الكيان/الجدول المتأثر.</summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>معرّف السجل المتأثر (كنص لدعم أنواع مفاتيح مختلفة).</summary>
    public string? EntityId { get; set; }

    /// <summary>القيم قبل التعديل (JSON).</summary>
    public string? OldValues { get; set; }

    /// <summary>القيم بعد التعديل (JSON).</summary>
    public string? NewValues { get; set; }

    public DateTime Timestamp { get; set; }

    public string? IpAddress { get; set; }
}
