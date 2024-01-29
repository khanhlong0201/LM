using System.ComponentModel.DataAnnotations;

namespace LM.Models;

public class BorrowingModel : Auditable
{
    public int BorrowingId { get; set; }
    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public decimal TotalFine { get; set; }
    public int UserId { get; set; }
    public int BookId { get; set; }
}

