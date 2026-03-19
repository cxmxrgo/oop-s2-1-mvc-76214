using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace library.MVC.Models;

public class LoanCreateViewModel
{
    [Required]
    [Display(Name = "Book")]
    public int BookId { get; set; }

    [Required]
    [Display(Name = "Member")]
    public int MemberId { get; set; }

    [DataType(DataType.Date)]
    public DateTime LoanDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);

    public List<SelectListItem> AvailableBooks { get; set; } = new();

    public List<SelectListItem> Members { get; set; } = new();
}
