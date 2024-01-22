using System.ComponentModel.DataAnnotations;

namespace LM.Models;

public class ReaderModel : Auditable
{
    public int ReaderId { get; set; }
    public string FullName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; }
    public string Level { get; set; }
    public string Job { get; set; }
    public string WorkPlace { get; set; }
    public string NumOfPage { get; set; }
    public string IdentityCard { get; set; }
    public int UserId { get; set; }
}

