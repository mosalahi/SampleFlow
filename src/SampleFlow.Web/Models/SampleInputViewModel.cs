using System.ComponentModel.DataAnnotations;

namespace SampleFlow.Web.Models;

/// <summary>خانة إدخال لنوع تحليل واحد ضمن نموذج اليوم.</summary>
public sealed class SampleInputViewModel
{
    public int SampleTypeId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    [Range(0, 100000, ErrorMessage = "أدخل عدداً بين 0 و100000")]
    public int Count { get; set; }
}
