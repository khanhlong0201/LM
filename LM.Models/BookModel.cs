using System.ComponentModel.DataAnnotations;

namespace LM.Models;

public class BookModel : Auditable
{
    public int BookId { get; set; }
    public string BookName { get; set; }
    public string Description { get; set; }
    public decimal? Price { get; set; }
    public int? Qty { get; set; }
    public string Language { get; set; }
    public string Size { get; set; }
    public int? NumOfPage { get; set; }
    public int? KindBookId { get; set; }
    public int? PublisherId { get; set; }
    public int? ImageId { get; set; }
    public string PublisherName { get; set; }
    public string KindBookName { get; set; }
    public string FilePath { get; set; }
    public List<ImageDetailModel>? ListFile { get; set; }
}

