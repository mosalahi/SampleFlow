using System.ComponentModel.DataAnnotations;

namespace SampleFlow.Web.Models;

public sealed class SampleTypeFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "أدخل الرمز")]
    [StringLength(50)]
    [Display(Name = "الرمز")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "أدخل الاسم")]
    [StringLength(150)]
    [Display(Name = "الاسم المعروض")]
    public string Name { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    [Display(Name = "ترتيب الظهور")]
    public int DisplayOrder { get; set; }

    [Display(Name = "نشط")]
    public bool IsActive { get; set; } = true;
}
