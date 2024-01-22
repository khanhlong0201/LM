using System.ComponentModel.DataAnnotations;

namespace LM.Models;

public class UserModel : Auditable
{
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

    public DateTime? DateOfBirth { get; set; }
    public bool IsAdmin { get; set; }

    [Required(ErrorMessage = "Vui lòng điền nhập lại Mật khẩu")]
    public string? ReEnterPassword { get; set; }
    public string? PasswordNew { get; set; }
}

/// <summary>
/// model dùng để thay đổi mật khẩu vì Required khác nhau nên không thể sử dụng chung model
/// </summary>
public class UserProfileModel : Auditable
{
    public int Id { get; set; }
    public string? EmpNo { get; set; }

    // [Required(ErrorMessage = "Vui lòng điền Tên tài khoản")]
    public string? UserName { get; set; }

    [Required(ErrorMessage = "Vui lòng điền Mật khẩu")]
    public string? Password { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool IsAdmin { get; set; }

    [Required(ErrorMessage = "Vui lòng điền Mật khẩu mới")]
    public string? PasswordNew { get; set; }
    [Required(ErrorMessage = "Vui lòng điền nhập lại Mật khẩu mới")]
    public string? ReEnterPasswordNew { get; set; }
}

