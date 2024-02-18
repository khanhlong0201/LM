using System.ComponentModel.DataAnnotations;

namespace LM.Models;

public class LoginRequestModel
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public bool IsLogin { get; set; }
}
