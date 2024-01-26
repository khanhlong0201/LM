using System.ComponentModel.DataAnnotations;

namespace LM.Models;

public class ImageDetailModel : Auditable
{
    public int ImageDetailId { get; set; }
    public string FilePath { get; set; }
    public int ImageId { get; set; }
    public string ImageUrl { get; set; }
    public string FileName { get; set; }

}

