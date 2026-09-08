using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SampleFlow.Domain.Authorization;
using SampleFlow.Domain.Entities;
using SampleFlow.Infrastructure.Data;
using SampleFlow.Infrastructure.Identity;
using SampleFlow.Web.Models;

namespace SampleFlow.Web.Controllers;

[Authorize(Policy = PermissionKeys.UsersManage)]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public UsersController(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
        var centers = await _db.Centers.ToDictionaryAsync(c => c.Id, c => c.Name);

        var items = new List<UserListItemViewModel>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            items.Add(new UserListItemViewModel
            {
                Id = u.Id,
                Email = u.Email ?? u.UserName ?? "—",
                Roles = string.Join("، ", roles),
                CenterName = u.CenterId.HasValue && centers.TryGetValue(u.CenterId.Value, out var n) ? n : null,
            });
        }

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
        => View(await FillCreateOptionsAsync(new UserCreateViewModel { Role = Roles.Center }));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        ValidateRoleCenter(model.Role, model.CenterId);

        if (!ModelState.IsValid)
        {
            return View(await FillCreateOptionsAsync(model));
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            CenterId = model.Role == Roles.Center ? model.CenterId : null,
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return View(await FillCreateOptionsAsync(model));
        }

        await _userManager.AddToRoleAsync(user, model.Role);

        TempData["Success"] = "تمت إضافة المستخدم.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var vm = new UserEditViewModel
        {
            Id = user.Id,
            Email = user.Email ?? user.UserName ?? string.Empty,
            Role = roles.FirstOrDefault() ?? Roles.Center,
            CenterId = user.CenterId,
        };

        return View(await FillEditOptionsAsync(vm));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserEditViewModel model)
    {
        ValidateRoleCenter(model.Role, model.CenterId);

        var user = await _userManager.FindByIdAsync(model.Id);
        if (user is null)
        {
            return NotFound();
        }

        // منع المستخدم من إزالة دور الأدمن عن نفسه (تفادي القفل الذاتي).
        if (user.Id == _userManager.GetUserId(User) && model.Role != Roles.Admin
            && await _userManager.IsInRoleAsync(user, Roles.Admin))
        {
            ModelState.AddModelError(nameof(model.Role), "لا يمكنك إزالة دور الأدمن عن حسابك.");
        }

        if (!ModelState.IsValid)
        {
            return View(await FillEditOptionsAsync(model));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, model.Role);

        user.CenterId = model.Role == Roles.Center ? model.CenterId : null;
        await _userManager.UpdateAsync(user);

        TempData["Success"] = "تم حفظ بيانات المستخدم.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Permissions(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var permissions = await _db.Permissions.OrderBy(p => p.Category).ThenBy(p => p.Id).ToListAsync();
        var roleKeys = await RolePermissionKeysAsync(user);
        var overrides = await _db.UserPermissions
            .Where(up => up.UserId == user.Id)
            .ToDictionaryAsync(up => up.PermissionId, up => up.IsGranted);

        var vm = new UserPermissionsViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? user.UserName ?? string.Empty,
            Rows = permissions.Select(p => new PermissionRowInput
            {
                PermissionId = p.Id,
                Key = p.Key,
                Name = p.Name,
                Category = p.Category,
                RoleGrants = roleKeys.Contains(p.Key),
                State = overrides.TryGetValue(p.Id, out var granted)
                    ? (granted ? "grant" : "revoke")
                    : "inherit",
            }).ToList(),
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Permissions(UserPermissionsViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user is null)
        {
            return NotFound();
        }

        var existing = await _db.UserPermissions
            .Where(up => up.UserId == user.Id)
            .ToDictionaryAsync(up => up.PermissionId, up => up);

        foreach (var row in model.Rows)
        {
            existing.TryGetValue(row.PermissionId, out var current);

            switch (row.State)
            {
                case "grant":
                    if (current is null)
                    {
                        _db.UserPermissions.Add(new UserPermission
                        {
                            UserId = user.Id,
                            PermissionId = row.PermissionId,
                            IsGranted = true,
                        });
                    }
                    else
                    {
                        current.IsGranted = true;
                    }
                    break;

                case "revoke":
                    if (current is null)
                    {
                        _db.UserPermissions.Add(new UserPermission
                        {
                            UserId = user.Id,
                            PermissionId = row.PermissionId,
                            IsGranted = false,
                        });
                    }
                    else
                    {
                        current.IsGranted = false;
                    }
                    break;

                default: // inherit
                    if (current is not null)
                    {
                        _db.UserPermissions.Remove(current);
                    }
                    break;
            }
        }

        await _db.SaveChangesAsync();

        TempData["Success"] = "تم حفظ الصلاحيات (تُطبّق فوراً).";
        return RedirectToAction(nameof(Index));
    }

    private async Task<HashSet<string>> RolePermissionKeysAsync(ApplicationUser user)
    {
        var roleNames = await _userManager.GetRolesAsync(user);
        var roleIds = await _db.Roles
            .Where(r => roleNames.Contains(r.Name!))
            .Select(r => r.Id)
            .ToListAsync();

        var keys = await _db.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.Key)
            .ToListAsync();

        return keys.ToHashSet(StringComparer.Ordinal);
    }

    private void ValidateRoleCenter(string role, int? centerId)
    {
        if (role == Roles.Center && !centerId.HasValue)
        {
            ModelState.AddModelError(nameof(UserCreateViewModel.CenterId), "دور المركز يتطلّب اختيار مركز.");
        }
    }

    private async Task<UserCreateViewModel> FillCreateOptionsAsync(UserCreateViewModel model)
    {
        model.RoleOptions = RoleOptions(model.Role);
        model.CenterOptions = await CenterOptionsAsync(model.CenterId);
        return model;
    }

    private async Task<UserEditViewModel> FillEditOptionsAsync(UserEditViewModel model)
    {
        model.RoleOptions = RoleOptions(model.Role);
        model.CenterOptions = await CenterOptionsAsync(model.CenterId);
        return model;
    }

    private static List<SelectListItem> RoleOptions(string? selected)
        => Roles.All.Select(r => new SelectListItem(r, r, r == selected)).ToList();

    private async Task<List<SelectListItem>> CenterOptionsAsync(int? selected)
        => await _db.Centers
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == selected))
            .ToListAsync();

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
