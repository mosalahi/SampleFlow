using System.ComponentModel.DataAnnotations;

namespace SampleFlow.Web.Models;

public sealed class CenterFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "أدخل اسم المركز")]
    [StringLength(150)]
    [Display(Name = "اسم المركز")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "نشط")]
    public bool IsActive { get; set; } = true;
}
