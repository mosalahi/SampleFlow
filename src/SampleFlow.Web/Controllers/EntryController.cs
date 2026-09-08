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
    // نافذة الإدخال المسموحة: اليوم وحتى هذا العدد من الأيام للخلف (لا مستقبل).
    private const int MaxBackDays = 7;

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

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
    private static DateOnly Earliest => Today.AddDays(-MaxBackDays);

    [HttpGet]
    public async Task<IActionResult> Index(DateOnly? date, int? centerId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var perms = await _permissions.GetEffectivePermissionsAsync(user.Id);

        // قصر التاريخ المعروض ضمن النافذة المسموحة.
        var entryDate = date ?? Today;
        if (entryDate > Today)
        {
            entryDate = Today;
        }
        else if (entryDate < Earliest)
        {
            entryDate = Earliest;
        }

        var vm = new DailyEntryViewModel { EntryDate = entryDate };
        SetDateBounds(vm);
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

        // تحقّق نافذة التاريخ على الخادم (لا مستقبل، ولا أقدم من الحد).
        if (model.EntryDate > Today)
        {
            ModelState.AddModelError(nameof(model.EntryDate), "لا يمكن اختيار تاريخ مستقبلي.");
        }
        else if (model.EntryDate < Earliest)
        {
            ModelState.AddModelError(nameof(model.EntryDate), $"لا يمكن الإدخال لتاريخ أقدم من {MaxBackDays} أيام.");
        }

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

        var isNew = existing is null;

        if (isNew)
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
        }
        else
        {
            if (!CanEditEntry(perms, user, existing!.CenterId))
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
        }

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            _db.ChangeTracker.Clear();
            ModelState.AddModelError(string.Empty, "تعذّر الحفظ — قد يكون هناك إدخال آخر لنفس المركز واليوم.");
            return await RebuildAsync(model, user, centerId);
        }

        // رسالة النجاح تُضبط بعد نجاح الحفظ فقط.
        TempData["Success"] = isNew ? "تم حفظ إدخال اليوم." : "تم تحديث إدخال اليوم.";
        return RedirectToAction(nameof(Index), new { date = model.EntryDate.ToString("yyyy-MM-dd"), centerId = RedirectCenter(user, centerId) });
    }

    private static void SetDateBounds(DailyEntryViewModel vm)
    {
        vm.MinDate = Earliest;
        vm.MaxDate = Today;
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
        SetDateBounds(model);

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

        // إعادة بناء خانات التحاليل من الأنواع النشطة مع الحفاظ على القيم المُدخلة،
        // حتى لا تختفي الحقول عند فشل التحقق.
        var activeTypes = await GetActiveSampleTypesAsync();
        var postedCounts = (model.Samples ?? new())
            .Where(s => s.SampleTypeId > 0)
            .ToDictionary(s => s.SampleTypeId, s => s.Count);

        model.Samples = activeTypes
            .Select(t => new SampleInputViewModel
            {
                SampleTypeId = t.Id,
                Code = t.Code,
                Name = t.Name,
                Count = postedCounts.TryGetValue(t.Id, out var c) ? c : 0,
            })
            .ToList();

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
