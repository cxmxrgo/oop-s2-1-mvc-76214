using Library.Domain;
using library.MVC.Areas.Admin.Controllers;
using library.MVC.Controllers;
using library.MVC.Data;
using library.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Tests;

public class LibraryBusinessRulesTests
{
    [Fact]
    public async Task CannotCreateLoanIfBookAlreadyOnActiveLoan()
    {
        await using var context = CreateContext();

        var book = new Book { Title = "Book 1", Author = "Author 1", IsAvailable = false };
        var member = new Member { FullName = "Member 1" };
        context.Books.Add(book);
        context.Members.Add(member);
        await context.SaveChangesAsync();

        context.Loans.Add(new Loan
        {
            BookId = book.Id,
            MemberId = member.Id,
            LoanDate = DateTime.Today.AddDays(-2),
            DueDate = DateTime.Today.AddDays(7),
            ReturnedDate = null
        });
        await context.SaveChangesAsync();

        var controller = new LoansController(context);

        var result = await controller.Create(new LoanCreateViewModel
        {
            BookId = book.Id,
            MemberId = member.Id,
            LoanDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(14)
        });

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Equal(1, await context.Loans.CountAsync());
        Assert.IsType<LoanCreateViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task ReturningLoanMakesBookAvailable()
    {
        await using var context = CreateContext();

        var book = new Book { Title = "Book 1", Author = "Author 1", IsAvailable = false };
        var member = new Member { FullName = "Member 1" };
        context.Books.Add(book);
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var loan = new Loan
        {
            BookId = book.Id,
            MemberId = member.Id,
            LoanDate = DateTime.Today.AddDays(-10),
            DueDate = DateTime.Today.AddDays(-3),
            ReturnedDate = null
        };

        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        var controller = new LoansController(context);
        await controller.MarkReturned(loan.Id);

        var updatedLoan = await context.Loans.Include(l => l.Book).FirstAsync();
        Assert.NotNull(updatedLoan.ReturnedDate);
        Assert.True(updatedLoan.Book.IsAvailable);
    }

    [Fact]
    public async Task BookSearchReturnsCorrectResults()
    {
        await using var context = CreateContext();

        context.Books.AddRange(
            new Book { Title = "The Hobbit", Author = "Tolkien", Category = "Fiction", IsAvailable = true },
            new Book { Title = "Clean Code", Author = "Robert Martin", Category = "Technology", IsAvailable = true },
            new Book { Title = "History 101", Author = "John Smith", Category = "History", IsAvailable = false }
        );
        await context.SaveChangesAsync();

        var controller = new BooksController(context);
        var result = await controller.Index("Hobbit", null, "All");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BookIndexViewModel>(viewResult.Model);

        Assert.Single(model.Books);
        Assert.Equal("The Hobbit", model.Books[0].Title);
    }

    [Fact]
    public async Task OverdueLogicReturnsOnlyActivePastDueLoans()
    {
        await using var context = CreateContext();

        var book = new Book { Title = "Book 1", Author = "Author 1", IsAvailable = false };
        var member = new Member { FullName = "Member 1" };
        context.Books.Add(book);
        context.Members.Add(member);
        await context.SaveChangesAsync();

        context.Loans.AddRange(
            new Loan
            {
                BookId = book.Id,
                MemberId = member.Id,
                LoanDate = DateTime.Today.AddDays(-20),
                DueDate = DateTime.Today.AddDays(-5),
                ReturnedDate = null
            },
            new Loan
            {
                BookId = book.Id,
                MemberId = member.Id,
                LoanDate = DateTime.Today.AddDays(-20),
                DueDate = DateTime.Today.AddDays(-2),
                ReturnedDate = DateTime.Today.AddDays(-1)
            },
            new Loan
            {
                BookId = book.Id,
                MemberId = member.Id,
                LoanDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(7),
                ReturnedDate = null
            }
        );
        await context.SaveChangesAsync();

        var overdueCount = await context.Loans.CountAsync(l => l.DueDate < DateTime.Today && l.ReturnedDate == null);
        Assert.Equal(1, overdueCount);
    }

    [Fact]
    public void AdminRolesControllerHasAdminAuthorizeAttribute()
    {
        var attribute = typeof(RolesController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        Assert.NotNull(attribute);
        Assert.Equal("Admin", attribute!.Roles);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
