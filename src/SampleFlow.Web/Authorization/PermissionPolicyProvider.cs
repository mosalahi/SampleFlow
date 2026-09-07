using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace SampleFlow.Web.Authorization;

/// <summary>
/// مزوّد سياسات ديناميكي: أي اسم سياسة (= مفتاح صلاحية مثل "Entries.EditAny")
/// يُبنى تلقائياً كسياسة تتطلّب تلك الصلاحية، دون تعريف كل سياسة يدوياً.
/// يرجع للمزوّد الافتراضي أولاً لدعم أي سياسات مُعرّفة صراحةً.
/// </summary>
public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        => _fallback = new DefaultAuthorizationPolicyProvider(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();

    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var existing = await _fallback.GetPolicyAsync(policyName);
        if (existing is not null)
        {
            return existing;
        }

        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();
    }
}
