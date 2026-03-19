using Library.Domain;

namespace library.MVC.Models;

public class BookIndexViewModel
{
    public string? SearchTerm { get; set; }

    public string? Category { get; set; }

    public string Availability { get; set; } = "All";

    public List<string> Categories { get; set; } = new();

    public List<Book> Books { get; set; } = new();
}
