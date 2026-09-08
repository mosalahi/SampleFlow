using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleFlow.Domain.Authorization;
using SampleFlow.Domain.Entities;
using SampleFlow.Infrastructure.Data;
using SampleFlow.Web.Models;

namespace SampleFlow.Web.Controllers;

[Authorize(Policy = PermissionKeys.CentersManage)]
public class CentersController : Controller
{
    private readonly ApplicationDbContext _db;

    public CentersController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var centers = await _db.Centers.OrderBy(c => c.Name).ToListAsync();
        return View(centers);
    }

    [HttpGet]
    public IActionResult Create() => View("Form", new CenterFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CenterFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        _db.Centers.Add(new Center
        {
            Name = model.Name.Trim(),
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();

        TempData["Success"] = "تمت إضافة المركز.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var center = await _db.Centers.FindAsync(id);
        if (center is null)
        {
            return NotFound();
        }

        return View("Form", new CenterFormViewModel
        {
            Id = center.Id,
            Name = center.Name,
            IsActive = center.IsActive,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CenterFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var center = await _db.Centers.FindAsync(model.Id);
        if (center is null)
        {
            return NotFound();
        }

        center.Name = model.Name.Trim();
        center.IsActive = model.IsActive;
        await _db.SaveChangesAsync();

        TempData["Success"] = "تم حفظ التعديلات.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var center = await _db.Centers.FindAsync(id);
        if (center is null)
        {
            return NotFound();
        }

        center.IsActive = !center.IsActive;
        await _db.SaveChangesAsync();

        TempData["Success"] = center.IsActive ? "تم تفعيل المركز." : "تم تعطيل المركز.";
        return RedirectToAction(nameof(Index));
    }
}
