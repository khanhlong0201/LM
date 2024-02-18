using System.ComponentModel.DataAnnotations;

namespace LM.Models;

public class BookModel : Auditable
{
    public int BookId { get; set; }

    [Required(ErrorMessage = "Vui lòng điền Tên sách")]
    public string? BookName { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Qty { get; set; }
    public string? Language { get; set; }
    public string? Size { get; set; }
    public int NumOfPage { get; set; }

    [Range(1, double.MaxValue, ErrorMessage = "Vui lòng chọn Loại sách")]
    public int KindBookId { get; set; }

    [Range(1, double.MaxValue, ErrorMessage = "Vui lòng chọn Nhà xuất bản")]
    public int PublisherId { get; set; }
    public int ImageId { get; set; }
    public string? PublisherName { get; set; }
    public string? KindBookName { get; set; }
    public string? FilePath { get; set; }

    [Range(1, double.MaxValue, ErrorMessage = "Vui lòng điền Năm xuất bản")]
    public int  PublishingYear { get; set; } // năm xuất bản
    public string? Name { get; set; }
    public string? AuthorName { get; set; }

    [Range(1, double.MaxValue, ErrorMessage = "Vui lòng chọn Tác giả")]
    public int AuthorId { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageUrlView { get; set; }
    public int QtyBO { get; set; }
}

public class CliBookModel : Auditable
{
    public int BookId { get; set; }
    public string? BookName { get; set; }
    public int PublisherId { get; set; }
    public string? PublisherName { get; set; }
    public int KindBookId { get; set; }
    public string? KindBookName { get; set; }
    public int BatchId { get; set; }
    public int SeriesId { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? Qty { get; set; }
    public string? Language { get; set; }
    public string? Size { get; set; }
    public string? Desciption { get; set; }
    public string? ImageUrlBook { get; set; } // ảnh đại diện cho sách
    public List<string>? ListImages { get; set; }
    public bool IsHorizontal { get; set; }
    public int ImageDetailId { get; set; }
    public string? FilePath { get; set; }
    public int ImageId { get; set; }
    public string? FullName { get; set; }
}
