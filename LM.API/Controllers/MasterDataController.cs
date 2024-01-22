using LM.API.Services;
using LM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterDataController : ControllerBase
    {
        
        private readonly IMasterDataService _masterService;
        private ILogger<MasterDataController> _logger { get; set; }
        private readonly IConfiguration _configuration;

        public MasterDataController(ILogger<MasterDataController> logger, IMasterDataService masterService, IConfiguration configuration)
        {
            _logger = logger;
            _masterService = masterService;
            _configuration = configuration;
        }

       
        [HttpGet]
        [Route("GetUsers")]
        public async Task<IActionResult> GetDataUsers(int pUserId=-1)
        {
            try
            {
                var data = await _masterService.GetUsersAsync(pUserId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MasterDataController", "GetDataUsers");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });
            }

        }

        [HttpPost]
        [Route("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] RequestModel request)
        {
            try
            {
                var response = await _masterService.UpdateUsers(request);
                if (response == null || response.StatusCode != 0) return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = response?.Message ?? "Vui lòng liên hệ IT để được hổ trợ."
                });
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MasterDataController", "UpdateUser");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });

            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody]  LoginRequestModel loginRequest)
        {
            try
            {
                var data = await _masterService.Login(loginRequest);
                if (data == null || data.Count() ==0) 
                return BadRequest(new {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Tên đăng nhập hoặc mật khẩu không hợp lệ"
                });
                UserModel oUser = data.First();
                if (oUser.IsDeleted == true )
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = $"Tài khoản này đã bị xóa. Bạn hãy liên hệ Quản Trị Viên để nhận tài khoản khác !"
                });

                var claims = new[]
                {
                    new Claim("UserId", oUser.Id + ""),
                    new Claim("UserName", oUser.UserName + ""),
                    new Claim("FullName", oUser.FullName + ""),
                    new Claim("IsAdmin", oUser.IsAdmin + ""),

                    new Claim(nameof(ClaimTypes.Role), oUser.IsAdmin ?  "administrator" : "staff"),
                }; // thông tin mã hóa (payload)
                // JWT: json web token: Header - Payload - SIGNATURE (base64UrlEncode(header) + "." + base64UrlEncode(payload), your - 256 - bit - secret)
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:JwtSecurityKey").Value + "")); // key mã hóa
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // loại mã hóa (Header)
                var expiry = DateTime.Now.AddDays(Convert.ToInt32(_configuration.GetSection("Jwt:JwtExpiryInDays").Value)); // hết hạn token
                //var expiry = DateTime.Now.AddMinutes(Convert.ToInt32(_configuration.GetSection("Jwt:JwtExpiryInDays").Value)); // hết hạn token test 1 phút hết tonken
                var token = new JwtSecurityToken(
                    _configuration.GetSection("Jwt:JwtIssuer").Value,
                    _configuration.GetSection("Jwt:JwtAudience").Value,
                    claims,
                    expires: expiry,
                    signingCredentials: creds
                );
                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Token = new JwtSecurityTokenHandler().WriteToken(token) // token user
                });

 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MasterDataController", "Login");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });
            }
        }

        [HttpPost]
        [Route("DeleteData")]
        public async Task<IActionResult> DeleteData([FromBody] RequestModel request)
        {
            try
            {
                var response = await _masterService.DeleteDataAsync(request);
                if (response == null || response.StatusCode != 0) return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = response?.Message ?? "Vui lòng liên hệ IT để được hổ trợ."
                });
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MasterDataController", "UpdateService");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });

            }
        }

        [HttpGet]
        [Route("GetKindBooks")]
        public async Task<IActionResult> GetDataKindBooks()
        {
            try
            {
                var data = await _masterService.GetKindBooksAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MasterDataController", "GetDataUsers");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });
            }

        }

        [HttpPost]
        [Route("UpdateKindBook")]
        public async Task<IActionResult> UpdateKindBook([FromBody] RequestModel request)
        {
            try
            {
                var response = await _masterService.UpdateKindBooks(request);
                if (response == null || response.StatusCode != 0) return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = response?.Message ?? "Vui lòng liên hệ IT để được hổ trợ."
                });
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MasterDataController", "UpdateUser");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });

            }
        }

        [HttpGet]
        [Route("GetPublishers")]
        public async Task<IActionResult> GetDataPublishers()
        {
            try
            {
                var data = await _masterService.GetPublishersAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MasterDataController", "GetPublishers");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });
            }

        }

        [HttpPost]
        [Route("UpdatePublisher")]
        public async Task<IActionResult> UpdatePublisher([FromBody] RequestModel request)
        {
            try
            {
                var response = await _masterService.UpdatePublishers(request);
                if (response == null || response.StatusCode != 0) return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = response?.Message ?? "Vui lòng liên hệ IT để được hổ trợ."
                });
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MasterDataController", "UpdateUser");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });

            }
        }

        [HttpGet]
        [Route("GetBooks")]
        public async Task<IActionResult> GetDataBooks()
        {
            try
            {
                var data = await _masterService.GetBooksAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MasterDataController", "GetDataBooks");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });
            }

        }

        [HttpPost]
        [Route("UpdateBook")]
        public async Task<IActionResult> UpdateBook([FromBody] RequestModel request)
        {
            try
            {
                var response = await _masterService.UpdateBooks(request);
                if (response == null || response.StatusCode != 0) return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = response?.Message ?? "Vui lòng liên hệ IT để được hổ trợ."
                });
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MasterDataController", "UpdateBook");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });

            }
        }
    }
}
