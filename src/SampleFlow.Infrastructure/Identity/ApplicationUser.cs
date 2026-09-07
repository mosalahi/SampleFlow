using Microsoft.AspNetCore.Identity;
using SampleFlow.Domain.Entities;

namespace SampleFlow.Infrastructure.Identity;

/// <summary>
/// مستخدم النظام. توسعة IdentityUser بربط المستخدم بمركزه.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// المركز المرتبط بحساب المركز. للمشرف/الأدمن يكون null (FK → Centers).
    /// </summary>
    public int? CenterId { get; set; }

    // Navigation
    public Center? Center { get; set; }
}
