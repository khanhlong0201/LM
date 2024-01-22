using System.ComponentModel.DataAnnotations;

namespace LM.WEB.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng điền tên đăng nhập")]
    public string? UserName { get; set; }
    [Required(ErrorMessage = "Vui lòng điền mật khẩu")]
    public string? Password { get; set; }

    //[Required(ErrorMessage = "Vui lòng chọn chi nhánh")]
    //public string? BranchId { get; set; }
    public bool RememberMe { get; set; } // ghi nhớ đăng nhập để tạo refresh token
    public string? RefreshToken { get; set; }
}
public class LoginResponseViewModel
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public int UserId { get; set; }
    public string? FullName { get; set; }

    public LoginResponseViewModel() { }
    public LoginResponseViewModel(int StatusCode, string Message)
    {
        this.StatusCode = StatusCode;
        this.Message = Message;
    }

}
