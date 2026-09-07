using System.Security.Claims;
using SampleFlow.Infrastructure.Auditing;

namespace SampleFlow.Web.Services;

/// <summary>يزوّد سجل التتبّع بمعلومات الفاعل من الطلب الحالي.</summary>
public sealed class HttpAuditInfoProvider : IAuditInfoProvider
{
    private readonly IHttpContextAccessor _accessor;

    public HttpAuditInfoProvider(IHttpContextAccessor accessor) => _accessor = accessor;

    private HttpContext? Context => _accessor.HttpContext;

    public string? UserId => Context?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName => Context?.User.Identity?.Name;

    public string? IpAddress => Context?.Connection.RemoteIpAddress?.ToString();
}
