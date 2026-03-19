using Library.Domain;
using library.MVC.Data;
using library.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace library.MVC.Controllers;

public class LoansController(ApplicationDbContext context) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var loans = await context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .OrderByDescending(l => l.LoanDate)
            .ToListAsync();

        return View(loans);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        var model = new LoanCreateViewModel();
        await PopulateDropDownsAsync(model);
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LoanCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropDownsAsync(model);
            return View(model);
        }

        var hasActiveLoan = await context.Loans
            .AnyAsync(l => l.BookId == model.BookId && l.ReturnedDate == null);

        if (hasActiveLoan)
        {
            ModelState.AddModelError(nameof(model.BookId), "This book is already on an active loan.");
            await PopulateDropDownsAsync(model);
            return View(model);
        }

        var book = await context.Books.FindAsync(model.BookId);
        if (book is null)
        {
            ModelState.AddModelError(nameof(model.BookId), "Selected book was not found.");
            await PopulateDropDownsAsync(model);
            return View(model);
        }

        var loan = new Loan
        {
            BookId = model.BookId,
            MemberId = model.MemberId,
            LoanDate = model.LoanDate,
            DueDate = model.DueDate,
            ReturnedDate = null
        };

        context.Loans.Add(loan);
        book.IsAvailable = false;

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkReturned(int id)
    {
        var loan = await context.Loans
            .Include(l => l.Book)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan is null)
        {
            return NotFound();
        }

        if (loan.ReturnedDate is null)
        {
            loan.ReturnedDate = DateTime.Now;
            loan.Book.IsAvailable = true;
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropDownsAsync(LoanCreateViewModel model)
    {
        model.AvailableBooks = await context.Books
            .Where(b => b.IsAvailable)
            .OrderBy(b => b.Title)
            .Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = $"{b.Title} ({b.Author})"
            })
            .ToListAsync();

        model.Members = await context.Members
            .OrderBy(m => m.FullName)
            .Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.FullName
            })
            .ToListAsync();
    }
}
