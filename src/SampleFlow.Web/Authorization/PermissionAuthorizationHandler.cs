using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace SampleFlow.Web.Authorization;

/// <summary>
/// يفحص المتطلّب مقابل الصلاحيات الفعّالة للمستخدم المقروءة من القاعدة.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissions;

    public PermissionAuthorizationHandler(IPermissionService permissions) => _permissions = permissions;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        if (await _permissions.HasPermissionAsync(userId, requirement.PermissionKey))
        {
            context.Succeed(requirement);
        }
    }
}
