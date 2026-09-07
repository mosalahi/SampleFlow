using Microsoft.AspNetCore.Authorization;

namespace SampleFlow.Web.Authorization;

/// <summary>متطلّب تفويض يمثّل صلاحية دقيقة واحدة بمفتاحها.</summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permissionKey) => PermissionKey = permissionKey;

    public string PermissionKey { get; }
}
