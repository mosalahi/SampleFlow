namespace SampleFlow.Domain.Entities;

/// <summary>
/// عدد العينات لنوع تحليل واحد ضمن إدخال يومي.
/// قيد فريد على (DailyEntryId + SampleTypeId): صف واحد لكل نوع داخل الإدخال.
/// </summary>
public class DailyEntryDetail
{
    public int Id { get; set; }

    public int DailyEntryId { get; set; }

    public int SampleTypeId { get; set; }

    /// <summary>عدد العينات لهذا النوع.</summary>
    public int Count { get; set; }

    // Navigation
    public DailyEntry DailyEntry { get; set; } = null!;
    public SampleType SampleType { get; set; } = null!;
}
