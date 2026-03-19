using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace library.MVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class RolesController(RoleManager<IdentityRole> roleManager) : Controller
{
    public IActionResult Index()
    {
        return View(roleManager.Roles.OrderBy(r => r.Name).ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            TempData["Error"] = "Role name is required.";
            return RedirectToAction(nameof(Index));
        }

        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName.Trim()));
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role is not null && role.Name != "Admin")
        {
            await roleManager.DeleteAsync(role);
        }

        return RedirectToAction(nameof(Index));
    }
}
