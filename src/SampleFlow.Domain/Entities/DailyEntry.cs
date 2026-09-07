namespace SampleFlow.Domain.Entities;

/// <summary>
/// رأس سجل الإدخال اليومي لمركز في يوم واحد.
/// قيد فريد على (CenterId + EntryDate): إدخال واحد لكل مركز/يوم.
/// </summary>
public class DailyEntry
{
    public int Id { get; set; }

    public int CenterId { get; set; }

    /// <summary>تاريخ الإدخال (ميلادي، بدون وقت).</summary>
    public DateOnly EntryDate { get; set; }

    /// <summary>عدد المراجعين.</summary>
    public int Visitors { get; set; }

    /// <summary>عدد الرحلات.</summary>
    public int Trips { get; set; }

    /// <summary>معرّف المستخدم المنشئ (FK → AspNetUsers).</summary>
    public string CreatedByUserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    /// <summary>آخر من عدّل السجل (FK → AspNetUsers)، إن وُجد.</summary>
    public string? UpdatedByUserId { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Center Center { get; set; } = null!;

    /// <summary>صف لكل نوع تحليل ضمن هذا الإدخال.</summary>
    public ICollection<DailyEntryDetail> Details { get; set; } = new List<DailyEntryDetail>();
}
