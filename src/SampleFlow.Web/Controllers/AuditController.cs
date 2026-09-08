using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleFlow.Domain.Authorization;
using SampleFlow.Infrastructure.Data;
using SampleFlow.Web.Models;

namespace SampleFlow.Web.Controllers;

[Authorize(Policy = PermissionKeys.AuditView)]
public class AuditController : Controller
{
    private const int PageSize = 50;

    private readonly ApplicationDbContext _db;

    public AuditController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(
        DateOnly? from, DateOnly? to, string? act, string? userName, string? entityName, int page = 1)
    {
        if (page < 1)
        {
            page = 1;
        }

        var query = _db.AuditLogs.AsNoTracking().AsQueryable();

        if (from.HasValue)
        {
            var fromUtc = from.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(a => a.Timestamp >= fromUtc);
        }

        if (to.HasValue)
        {
            var toUtc = to.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(a => a.Timestamp < toUtc);
        }

        if (!string.IsNullOrWhiteSpace(act))
        {
            query = query.Where(a => a.Action == act);
        }

        if (!string.IsNullOrWhiteSpace(userName))
        {
            query = query.Where(a => a.UserName != null && a.UserName.Contains(userName));
        }

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            query = query.Where(a => a.EntityName.Contains(entityName));
        }

        var total = await query.CountAsync();

        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        return View(new AuditIndexViewModel
        {
            From = from,
            To = to,
            Action = act,
            UserName = userName,
            EntityName = entityName,
            Page = page,
            PageSize = PageSize,
            TotalCount = total,
            Logs = logs,
        });
    }
}
