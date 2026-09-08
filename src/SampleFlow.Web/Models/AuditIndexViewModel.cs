using SampleFlow.Domain.Entities;

namespace SampleFlow.Web.Models;

public sealed class AuditIndexViewModel
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public string? Action { get; set; }
    public string? UserName { get; set; }
    public string? EntityName { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public List<AuditLog> Logs { get; set; } = new();

    public static readonly string[] Actions = { "Create", "Update", "Delete", "Login" };
}
