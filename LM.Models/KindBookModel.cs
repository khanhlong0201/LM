using System.ComponentModel.DataAnnotations;

namespace LM.Models;

public class KindBookModel : Auditable
{
    public int KindBookId { get; set; }
    public string KindBookName { get; set; }
    public string Description { get; set; }
}

