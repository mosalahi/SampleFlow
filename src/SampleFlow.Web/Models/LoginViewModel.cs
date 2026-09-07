using System.ComponentModel.DataAnnotations;

namespace SampleFlow.Web.Models;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "أدخل اسم المستخدم أو البريد")]
    [Display(Name = "اسم المستخدم أو البريد")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "أدخل كلمة المرور")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "تذكّرني")]
    public bool RememberMe { get; set; }
}
