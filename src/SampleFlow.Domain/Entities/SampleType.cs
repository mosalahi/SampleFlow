namespace SampleFlow.Domain.Entities;

/// <summary>
/// نوع تحليل/عينة. إضافة نوع جديد = صف جديد هنا، وشاشة الإدخال تبنيه تلقائياً دون تعديل كود.
/// </summary>
public class SampleType
{
    public int Id { get; set; }

    /// <summary>رمز فريد للتحليل (مثل CBC, HBA1C).</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>الاسم المعروض.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>ترتيب الظهور في شاشة الإدخال ولوحة المشرف.</summary>
    public int DisplayOrder { get; set; }

    /// <summary>تعطيل النوع بدل حذفه.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>تفاصيل الإدخالات التي تستخدم هذا النوع.</summary>
    public ICollection<DailyEntryDetail> Details { get; set; } = new List<DailyEntryDetail>();
}
