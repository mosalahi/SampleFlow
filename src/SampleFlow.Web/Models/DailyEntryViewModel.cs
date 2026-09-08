using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SampleFlow.Web.Models;

/// <summary>نموذج إدخال اليوم — حقوله الديناميكية تُبنى من الأنواع النشطة.</summary>
public sealed class DailyEntryViewModel
{
    /// <summary>معرّف الإدخال إن كان موجوداً (وضع التعديل).</summary>
    public int? ExistingEntryId { get; set; }

    public int CenterId { get; set; }

    public string CenterName { get; set; } = string.Empty;

    /// <summary>مقفل على مركز المستخدم؛ للأدمن غير المرتبط بمركز يكون قابلاً للاختيار.</summary>
    public bool IsCenterLocked { get; set; }

    public List<SelectListItem> CenterOptions { get; set; } = new();

    [DataType(DataType.Date)]
    [Display(Name = "التاريخ")]
    public DateOnly EntryDate { get; set; }

    /// <summary>حدود التاريخ المسموح بها في الواجهة.</summary>
    public DateOnly MinDate { get; set; }
    public DateOnly MaxDate { get; set; }

    [Range(0, 100000, ErrorMessage = "أدخل عدداً بين 0 و100000")]
    [Display(Name = "عدد المراجعين")]
    public int Visitors { get; set; }

    [Range(0, 100000, ErrorMessage = "أدخل عدداً بين 0 و100000")]
    [Display(Name = "عدد الرحلات")]
    public int Trips { get; set; }

    public List<SampleInputViewModel> Samples { get; set; } = new();

    /// <summary>هل النموذج قابل للتحرير في الحالة الراهنة (حسب الصلاحية).</summary>
    public bool CanEdit { get; set; }

    /// <summary>هل يوجد إدخال مسبق لنفس المركز/اليوم.</summary>
    public bool AlreadyExists { get; set; }

    public int TotalSamples => Samples?.Sum(s => s.Count) ?? 0;
}
