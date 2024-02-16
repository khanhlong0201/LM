using System.ComponentModel.DataAnnotations;

namespace LM.Models;

public class KindBookModel : Auditable
{
    public int KindBookId { get; set; }

    [Required(ErrorMessage = "Vui lòng điền Tên loại sách")]
    public string? KindBookName { get; set; }
    public string? Description { get; set; }
    public int LocationId { get; set; }
    public string? LocationName { get; set; }
}

