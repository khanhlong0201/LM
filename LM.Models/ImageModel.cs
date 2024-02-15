using System.ComponentModel.DataAnnotations;

namespace LM.Models;

//public class ImageModel : Auditable
//{
//    public int ImageId { get; set; }
//    public string Type { get; set; }
//}

public class ImageModel
{
    public int Id { get; set; }
    public int DocEntry { get; set; }
    public string? TableName { get; set; }
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? DateCreate { get; set; }
    public int? UserCreate { get; set; }
    public bool IsDelete { get; set; }
    public bool IsAdd { get; set; }
}

