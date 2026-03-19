using Library.Domain;
using library.MVC.Data;
using library.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace library.MVC.Controllers;

public class BooksController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? searchTerm, string? category, string availability = "All")
    {
        var booksQuery = context.Books.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            booksQuery = booksQuery.Where(b => b.Title.Contains(searchTerm) || b.Author.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            booksQuery = booksQuery.Where(b => b.Category == category);
        }

        booksQuery = availability switch
        {
            "Available" => booksQuery.Where(b => b.IsAvailable),
            "OnLoan" => booksQuery.Where(b => !b.IsAvailable),
            _ => booksQuery
        };

        var model = new BookIndexViewModel
        {
            SearchTerm = searchTerm,
            Category = category,
            Availability = availability,
            Categories = await context.Books
                .Where(b => !string.IsNullOrEmpty(b.Category))
                .Select(b => b.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync(),
            Books = await booksQuery
                .OrderBy(b => b.Title)
                .ToListAsync()
        };

        return View(model);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Book book)
    {
        if (!ModelState.IsValid)
        {
            return View(book);
        }

        context.Books.Add(book);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var book = await context.Books.FindAsync(id);
        if (book is null)
        {
            return NotFound();
        }

        return View(book);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Book book)
    {
        if (id != book.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(book);
        }

        context.Update(book);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var book = await context.Books.FirstOrDefaultAsync(b => b.Id == id);
        if (book is null)
        {
            return NotFound();
        }

        return View(book);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await context.Books.FindAsync(id);
        if (book is not null)
        {
            context.Books.Remove(book);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
