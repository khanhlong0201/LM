using System.ComponentModel.DataAnnotations;

namespace LM.Models;

public class BorrowingDetailModel : Auditable
{
    public int BorrowingDetailId { get; set; }
    public string Remark { get; set; }
    public string StatusBefore { get; set; }
    public string StatusAfter { get; set; }
    public decimal FineAmount { get; set; }
    public int BookId { get; set; }
    public int SeriesId { get; set; }
    public int BorrowingId { get; set; }
    public string BookName { get; set; }
    public string SeriesName { get; set; }
    public string BorrowingName { get; set; }
}


