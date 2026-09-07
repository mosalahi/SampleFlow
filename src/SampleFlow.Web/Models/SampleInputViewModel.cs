using System.ComponentModel.DataAnnotations;

namespace SampleFlow.Web.Models;

/// <summary>خانة إدخال لنوع تحليل واحد ضمن نموذج اليوم.</summary>
public sealed class SampleInputViewModel
{
    public int SampleTypeId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "القيمة يجب أن تكون صفراً أو أكثر")]
    public int Count { get; set; }
}
