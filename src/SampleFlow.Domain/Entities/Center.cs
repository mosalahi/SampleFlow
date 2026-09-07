namespace SampleFlow.Domain.Entities;

/// <summary>
/// مركز صحي. تُدار المراكز بالكامل من لوحة الأدمن (إضافة/تعطيل) بدون تعديل الكود.
/// </summary>
public class Center
{
    public int Id { get; set; }

    /// <summary>اسم المركز.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>تعطيل المركز بدل حذفه (يحافظ على الإدخالات التاريخية).</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    /// <summary>إدخالات هذا المركز اليومية.</summary>
    public ICollection<DailyEntry> DailyEntries { get; set; } = new List<DailyEntry>();
}
