using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SampleFlow.Domain.Authorization;
using SampleFlow.Domain.Entities;
using SampleFlow.Infrastructure.Data;
using SampleFlow.Infrastructure.Identity;
using SampleFlow.Web.Authorization;
using SampleFlow.Web.Models;

namespace SampleFlow.Web.Controllers;

// شاشة إدخال المركز اليومي — تتطلّب صلاحية إضافة إدخال (Center/Admin).
[Authorize(Policy = PermissionKeys.EntriesCreate)]
public class EntryController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPermissionService _permissions;

    public EntryController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IPermissionService permissions)
    {
        _db = db;
        _userManager = userManager;
        _permissions = permissions;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DateOnly? date, int? centerId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var perms = await _permissions.GetEffectivePermissionsAsync(user.Id);
        var entryDate = date ?? DateOnly.FromDateTime(DateTime.Today);

        var vm = new DailyEntryViewModel { EntryDate = entryDate };
        await ResolveCenterAsync(vm, user, centerId);

        var activeTypes = await GetActiveSampleTypesAsync();

        DailyEntry? existing = vm.CenterId > 0
            ? await _db.DailyEntries
                .Include(e => e.Details)
                .FirstOrDefaultAsync(e => e.CenterId == vm.CenterId && e.EntryDate == entryDate)
            : null;

        if (existing is not null)
        {
            vm.ExistingEntryId = existing.Id;
            vm.AlreadyExists = true;
            vm.Visitors = existing.Visitors;
            vm.Trips = existing.Trips;
            vm.CanEdit = CanEditEntry(perms, user, existing.CenterId);
        }
        else
        {
            vm.CanEdit = perms.Contains(PermissionKeys.EntriesCreate);
        }

        vm.Samples = activeTypes
            .Select(t => new SampleInputViewModel
            {
                SampleTypeId = t.Id,
                Code = t.Code,
                Name = t.Name,
                Count = existing?.Details.FirstOrDefault(d => d.SampleTypeId == t.Id)?.Count ?? 0,
            })
            .ToList();

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(DailyEntryViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var perms = await _permissions.GetEffectivePermissionsAsync(user.Id);

        // فرض المركز: مستخدم المركز مقفل على مركزه؛ الأدمن يختار مركزاً نشطاً.
        var centerId = user.CenterId ?? model.CenterId;
        if (!user.CenterId.HasValue &&
            !await _db.Centers.AnyAsync(c => c.Id == centerId && c.IsActive))
        {
            ModelState.AddModelError(nameof(model.CenterId), "اختر مركزاً صالحاً.");
        }

        if (!ModelState.IsValid)
        {
            return await RebuildAsync(model, user, centerId);
        }

        var existing = await _db.DailyEntries
            .Include(e => e.Details)
            .FirstOrDefaultAsync(e => e.CenterId == centerId && e.EntryDate == model.EntryDate);

        if (existing is null)
        {
            if (!perms.Contains(PermissionKeys.EntriesCreate))
            {
                return Forbid();
            }

            _db.DailyEntries.Add(new DailyEntry
            {
                CenterId = centerId,
                EntryDate = model.EntryDate,
                Visitors = model.Visitors,
                Trips = model.Trips,
                CreatedByUserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                Details = model.Samples
                    .Select(s => new DailyEntryDetail { SampleTypeId = s.SampleTypeId, Count = s.Count })
                    .ToList(),
            });

            TempData["Success"] = "تم حفظ إدخال اليوم.";
        }
        else
        {
            if (!CanEditEntry(perms, user, existing.CenterId))
            {
                TempData["Error"] = "لا تملك صلاحية تعديل إدخال هذا اليوم.";
                return RedirectToAction(nameof(Index), new { date = model.EntryDate.ToString("yyyy-MM-dd"), centerId = RedirectCenter(user, centerId) });
            }

            existing.Visitors = model.Visitors;
            existing.Trips = model.Trips;
            existing.UpdatedByUserId = user.Id;
            existing.UpdatedAt = DateTime.UtcNow;

            foreach (var s in model.Samples)
            {
                var detail = existing.Details.FirstOrDefault(d => d.SampleTypeId == s.SampleTypeId);
                if (detail is null)
                {
                    existing.Details.Add(new DailyEntryDetail { SampleTypeId = s.SampleTypeId, Count = s.Count });
                }
                else
                {
                    detail.Count = s.Count;
                }
            }

            TempData["Success"] = "تم تحديث إدخال اليوم.";
        }

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "تعذّر الحفظ — قد يكون هناك إدخال آخر لنفس المركز واليوم.");
            return await RebuildAsync(model, user, centerId);
        }

        return RedirectToAction(nameof(Index), new { date = model.EntryDate.ToString("yyyy-MM-dd"), centerId = RedirectCenter(user, centerId) });
    }

    private async Task ResolveCenterAsync(DailyEntryViewModel vm, ApplicationUser user, int? requestedCenterId)
    {
        if (user.CenterId.HasValue)
        {
            vm.IsCenterLocked = true;
            vm.CenterId = user.CenterId.Value;
            vm.CenterName = (await _db.Centers.FindAsync(user.CenterId.Value))?.Name ?? string.Empty;
            return;
        }

        // الأدمن غير المرتبط بمركز — اختيار من المراكز النشطة (لأغراض الإدارة/التجربة).
        var centers = await _db.Centers
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        vm.IsCenterLocked = false;
        vm.CenterOptions = centers
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToList();
        vm.CenterId = requestedCenterId ?? centers.FirstOrDefault()?.Id ?? 0;
        vm.CenterName = centers.FirstOrDefault(c => c.Id == vm.CenterId)?.Name ?? string.Empty;
    }

    private async Task<IActionResult> RebuildAsync(DailyEntryViewModel model, ApplicationUser user, int centerId)
    {
        if (user.CenterId.HasValue)
        {
            model.IsCenterLocked = true;
            model.CenterId = user.CenterId.Value;
            model.CenterName = (await _db.Centers.FindAsync(user.CenterId.Value))?.Name ?? string.Empty;
        }
        else
        {
            var centers = await _db.Centers.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
            model.IsCenterLocked = false;
            model.CenterOptions = centers.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
            model.CenterName = centers.FirstOrDefault(c => c.Id == centerId)?.Name ?? string.Empty;
        }

        model.CanEdit = true;
        return View(model);
    }

    private async Task<List<SampleType>> GetActiveSampleTypesAsync()
        => await _db.SampleTypes
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();

    private static bool CanEditEntry(IReadOnlySet<string> perms, ApplicationUser user, int entryCenterId)
    {
        var isOwnCenter = user.CenterId.HasValue && user.CenterId.Value == entryCenterId;
        return perms.Contains(PermissionKeys.EntriesEditAny)
               || (perms.Contains(PermissionKeys.EntriesEditOwn) && isOwnCenter);
    }

    // للأدمن نمرّر centerId في التوجيه للحفاظ على المركز المختار؛ لمستخدم المركز لا داعي.
    private static int? RedirectCenter(ApplicationUser user, int centerId)
        => user.CenterId.HasValue ? null : centerId;
}
