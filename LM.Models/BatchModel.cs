using System.ComponentModel.DataAnnotations;

namespace LM.Models;

public class BatchModel : Auditable
{
    public int BatchId { get; set; }
    public int Qty { get; set; }
    public decimal Price { get; set; }
    public int BookId { get; set; }
    public string BookName { get; set; }
    public int KindBookId { get; set; }
    public int PublisherId { get; set; }
    public string KindBookName { get; set; }
    public string PublisherName { get; set; }
    public string Name { get; set; }

}

