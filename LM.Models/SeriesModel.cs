using System.ComponentModel.DataAnnotations;

namespace LM.Models;

public class SeriesModel : Auditable
{
    public int SeriesId { get; set; }
    public string SeriesCode { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }
    public int BatchId { get; set; }
    public int BookId { get; set; }
    public string BookName { get; set; }
    public int KindBookId { get; set; }
    public string KindBookName { get; set; }
    public int PublisherId { get; set; }
    public string PublisherName { get; set; }
    public string UserCreateName { get; set; }
    public int STT { get; set; }
}
