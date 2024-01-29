using System.ComponentModel.DataAnnotations;

namespace LM.Models;

public class ReaderModel : Auditable
{
    public int ReaderId { get; set; }
    //public string FullName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Gender { get; set; }
    public string Level { get; set; }
    public string Job { get; set; }
    public string WorkPlace { get; set; }
    public string NumOfPage { get; set; }
    public string IdentityCard { get; set; }
    public int UserId { get; set; }
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng điền Tên tài khoản")]
    public string? UserName { get; set; }

    [Required(ErrorMessage = "Vui lòng điền Mật khẩu")]
    public string? Password { get; set; }

    public string? LastPassword { get; set; }

    [Required(ErrorMessage = "Vui lòng điền Tên nhân viên")]
    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }

    //public DateTime? DateOfBirth { get; set; }
    public bool IsAdmin { get; set; }

    [Required(ErrorMessage = "Vui lòng điền nhập lại Mật khẩu")]
    public string? ReEnterPassword { get; set; }
    public string? PasswordNew { get; set; }
}

