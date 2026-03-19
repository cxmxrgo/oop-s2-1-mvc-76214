using System.ComponentModel.DataAnnotations;

namespace Library.Domain;

public class Member
{
    public int Id { get; set; }

    [Required]
    public string FullName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
