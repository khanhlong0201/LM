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
    public string Status { get; set; }
    public string FullName { get; set; }
    public string Phone { get; set; }

    /// <summary>
    /// /
    /// </summary>
    public string? BookName { get; set; }
    public int PublisherId { get; set; }
    public string? PublisherName { get; set; }
    public int KindBookId { get; set; }
    public string? KindBookName { get; set; }
    public int BatchId { get; set; }
    public int SeriesId { get; set; }
    public DateTime? DateCreate { get; set; }
    public string Description { get; set; }
    public decimal? Price { get; set; }
    public int? Qty { get; set; }
    public string Language { get; set; }
    public string Size { get; set; }
    public string? Desciption { get; set; }
    public string? ImageUrlBook { get; set; } // ảnh đại diện cho sách
    public List<string>? ListImages { get; set; }
    public bool IsHorizontal { get; set; }
    public int ImageDetailId { get; set; }
    public string FilePath { get; set; }
    public int ImageId { get; set; }
}

