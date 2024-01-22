using System.ComponentModel.DataAnnotations;

namespace LM.Models;

public class ImageModel : Auditable
{
    public int ImageId { get; set; }
    public string Type { get; set; }
}

