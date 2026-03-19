using Library.Domain;
using library.MVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace library.MVC.Controllers;

public class MembersController(ApplicationDbContext context) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        return View(await context.Members.OrderBy(m => m.FullName).ToListAsync());
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Member member)
    {
        if (!ModelState.IsValid)
        {
            return View(member);
        }

        context.Members.Add(member);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var member = await context.Members.FindAsync(id);
        if (member is null)
        {
            return NotFound();
        }

        return View(member);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Member member)
    {
        if (id != member.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(member);
        }

        context.Update(member);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var member = await context.Members.FirstOrDefaultAsync(m => m.Id == id);
        if (member is null)
        {
            return NotFound();
        }

        return View(member);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var member = await context.Members.FindAsync(id);
        if (member is not null)
        {
            context.Members.Remove(member);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
