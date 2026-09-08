using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleFlow.Domain.Authorization;
using SampleFlow.Domain.Entities;
using SampleFlow.Infrastructure.Data;
using SampleFlow.Web.Models;

namespace SampleFlow.Web.Controllers;

[Authorize(Policy = PermissionKeys.SampleTypesManage)]
public class SampleTypesController : Controller
{
    private readonly ApplicationDbContext _db;

    public SampleTypesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var types = await _db.SampleTypes
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Code)
            .ToListAsync();
        return View(types);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var nextOrder = (await _db.SampleTypes.MaxAsync(s => (int?)s.DisplayOrder) ?? 0) + 1;
        return View("Form", new SampleTypeFormViewModel { DisplayOrder = nextOrder });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SampleTypeFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        _db.SampleTypes.Add(new SampleType
        {
            Code = model.Code.Trim(),
            Name = model.Name.Trim(),
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive,
        });

        if (!await TrySaveAsync(model))
        {
            return View("Form", model);
        }

        TempData["Success"] = "تمت إضافة نوع التحليل.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var type = await _db.SampleTypes.FindAsync(id);
        if (type is null)
        {
            return NotFound();
        }

        return View("Form", new SampleTypeFormViewModel
        {
            Id = type.Id,
            Code = type.Code,
            Name = type.Name,
            DisplayOrder = type.DisplayOrder,
            IsActive = type.IsActive,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SampleTypeFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var type = await _db.SampleTypes.FindAsync(model.Id);
        if (type is null)
        {
            return NotFound();
        }

        type.Code = model.Code.Trim();
        type.Name = model.Name.Trim();
        type.DisplayOrder = model.DisplayOrder;
        type.IsActive = model.IsActive;

        if (!await TrySaveAsync(model))
        {
            return View("Form", model);
        }

        TempData["Success"] = "تم حفظ التعديلات.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var type = await _db.SampleTypes.FindAsync(id);
        if (type is null)
        {
            return NotFound();
        }

        type.IsActive = !type.IsActive;
        await _db.SaveChangesAsync();

        TempData["Success"] = type.IsActive ? "تم التفعيل." : "تم التعطيل.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> TrySaveAsync(SampleTypeFormViewModel model)
    {
        try
        {
            await _db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            _db.ChangeTracker.Clear();
            ModelState.AddModelError(nameof(model.Code), "الرمز مستخدم مسبقاً — اختر رمزاً فريداً.");
            return false;
        }
    }
}
