using Bogus;
using Library.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace library.MVC.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        await SeedAdminAsync(roleManager, userManager);
        await SeedLibraryDataAsync(context);
    }

    private static async Task SeedAdminAsync(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
    {
        const string adminRole = "Admin";
        const string adminEmail = "admin@test.com";
        const string adminPassword = "Letmein123!";

        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(adminUser, adminPassword);
        }
        else
        {
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            await userManager.ResetPasswordAsync(adminUser, resetToken, adminPassword);
        }

        if (!await userManager.IsInRoleAsync(adminUser, adminRole))
        {
            await userManager.AddToRoleAsync(adminUser, adminRole);
        }
    }

    private static async Task SeedLibraryDataAsync(ApplicationDbContext context)
    {
        if (await context.Books.AnyAsync() || await context.Members.AnyAsync() || await context.Loans.AnyAsync())
        {
            return;
        }

        var categories = new[] { "Fiction", "Science", "History", "Technology", "Biography" };

        var books = new Faker<Book>()
            .RuleFor(b => b.Title, f => f.Lorem.Sentence(3))
            .RuleFor(b => b.Author, f => f.Name.FullName())
            .RuleFor(b => b.Isbn, f => f.Commerce.Ean13())
            .RuleFor(b => b.Category, f => f.PickRandom(categories))
            .RuleFor(b => b.IsAvailable, _ => true)
            .Generate(20);

        var members = new Faker<Member>()
            .RuleFor(m => m.FullName, f => f.Name.FullName())
            .RuleFor(m => m.Email, f => f.Internet.Email())
            .RuleFor(m => m.Phone, f => f.Phone.PhoneNumber())
            .Generate(10);

        // TEST

        context.Books.AddRange(books);
        context.Members.AddRange(members);
        await context.SaveChangesAsync();

        var random = new Random();
        var selectedBooks = books.OrderBy(_ => random.Next()).Take(15).ToList();

        var loans = new List<Loan>();

        for (var i = 0; i < 15; i++)
        {
            var loanDate = DateTime.UtcNow.Date.AddDays(-random.Next(1, 30));
            var dueDate = loanDate.AddDays(14);

            DateTime? returnedDate = null;
            if (i < 6)
            {
                returnedDate = dueDate.AddDays(-random.Next(1, 5));
            }
            else if (i < 10)
            {
                dueDate = DateTime.UtcNow.Date.AddDays(-random.Next(1, 10));
            }
            else
            {
                dueDate = DateTime.UtcNow.Date.AddDays(random.Next(1, 14));
            }

            loans.Add(new Loan
            {
                BookId = selectedBooks[i].Id,
                MemberId = members[random.Next(members.Count)].Id,
                LoanDate = loanDate,
                DueDate = dueDate,
                ReturnedDate = returnedDate
            });
        }

        context.Loans.AddRange(loans);
        await context.SaveChangesAsync();

        var activeLoanBookIds = await context.Loans
            .Where(l => l.ReturnedDate == null)
            .Select(l => l.BookId)
            .Distinct()
            .ToListAsync();

        foreach (var book in books)
        {
            book.IsAvailable = !activeLoanBookIds.Contains(book.Id);
        }

        await context.SaveChangesAsync();
    }
}
