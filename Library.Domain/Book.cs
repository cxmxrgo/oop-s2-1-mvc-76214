using System.ComponentModel.DataAnnotations;

namespace Library.Domain;

public class Book
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Author { get; set; } = string.Empty;

    public string? Isbn { get; set; }

    public string? Category { get; set; }

    public bool IsAvailable { get; set; } = true;

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
