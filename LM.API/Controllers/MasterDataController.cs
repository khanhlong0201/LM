using LM.API.Services;
using LM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Image = SixLabors.ImageSharp.Image;

namespace LM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterDataController : ControllerBase
    {
        
        private readonly IMasterDataService _masterService;
        private ILogger<MasterDataController> _logger { get; set; }
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MasterDataController(ILogger<MasterDataController> logger, IMasterDataService masterService, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _masterService = masterService;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
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

        [HttpPost]
        [Route("GetBooks")]
        public async Task<IActionResult> GetDataBooks([FromBody] SearchModel pSearchData)
        {
            try
            {
                var data = await _masterService.GetBooksAsync(pSearchData);
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

        [HttpPost]
        [Route("UploadImages")]
        public async Task<IActionResult> UploadImages([FromForm] List<IFormFile> files, string subFolder)
        {
            try
            {
                if (files == null || !files.Any())
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Không có dữ liệu file đính kèm"
                    });
                }
                //
                var result = new List<ImageDetailModel>();
                string fileName = string.Empty;
                string path = $"{this._webHostEnvironment.WebRootPath}\\{subFolder}";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                foreach (var file in files)
                {
                    fileName = file.FileName; // trên kia mã hóa
                    string fullPath = Path.Combine(path, fileName);
                    using (var image = Image.Load(file.OpenReadStream()))
                    {
                        image.Mutate(m => m.Resize(810, 540));
                        await image.SaveAsync(fullPath);
                    }
                    result.Add(new ImageDetailModel() { FileName = fileName, FilePath = fullPath });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserController", "Update");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });

            }
        }

        [HttpGet]
        [Route("GetImageDetails")]
        public async Task<IActionResult> GetDataImageDetails(int imageId)
        {
            try
            {
                var data = await _masterService.GetImageDetailAsync(imageId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MasterDataController", "GetDataImageDetails");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });
            }

        }

        [HttpGet]
        [Route("GetReaders")]
        public async Task<IActionResult> GetDataReaders()
        {
            try
            {
                var data = await _masterService.GetReadersAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MasterDataController", "GetDataReaders");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });
            }

        }
        [HttpGet]
        [Route("GetBatchs")]
        public async Task<IActionResult> GetDataBatchs(int bookId)
        {
            try
            {
                var data = await _masterService.GetBatchsAsync(bookId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MasterDataController", "GetDataBatchs");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });
            }

        }

        [HttpPost]
        [Route("UpdateBatch")]
        public async Task<IActionResult> UpdateBatch([FromBody] RequestModel request)
        {
            try
            {
                var response = await _masterService.UpdateBatchs(request);
                if (response == null || response.StatusCode != 0) return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = response?.Message ?? "Vui lòng liên hệ IT để được hổ trợ."
                });
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MasterDataController", "UpdateBatch");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });

            }
        }

        [HttpGet]
        [Route("GetSeries")]
        public async Task<IActionResult> GetDataSeries(int batchId)
        {
            try
            {
                var data = await _masterService.GetSeriesAsync(batchId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MasterDataController", "GetDataSeries");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });
            }

        }

        [HttpPost]
        [Route("UpdateSeries")]
        public async Task<IActionResult> UpdateSeries([FromBody] RequestModel request)
        {
            try
            {
                var response = await _masterService.UpdateSeries(request);

                if (response == null || response.StatusCode != 0) return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = response?.Message ?? "Vui lòng liên hệ IT để được hổ trợ."
                });
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DocumentController", "UpdateSalesOrder");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });

            }
        }

        [HttpPost]
        [Route("GetBookClients")]
        public async Task<IActionResult> GetDataBookClients([FromBody] SearchModel pSearchData)
        {
            try
            {
                var data = await _masterService.GetBookClientsAsync(pSearchData);
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

        //
        [HttpGet]
        [Route("GetLocations")]
        public async Task<IActionResult> GetLocations()
        {
            try
            {
                var data = await _masterService.GetLocationsAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MasterDataController", "GetLocations");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });
            }

        }

        [HttpPost]
        [Route("UpdateLocation")]
        public async Task<IActionResult> UpdateLocation([FromBody] RequestModel request)
        {
            try
            {
                var response = await _masterService.UpdateLocationAsync(request);
                if (response == null || response.StatusCode != 0) return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = response?.Message ?? "Vui lòng liên hệ IT để được hổ trợ."
                });
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MasterDataController", "UpdateLocation");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });

            }
        }

        [HttpGet]
        [Route("GetAuthors")]
        public async Task<IActionResult> GetAuthors()
        {
            try
            {
                var data = await _masterService.GetAuthorsAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MasterDataController", "GetAuthors");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });
            }

        }

        [HttpPost]
        [Route("UpdateAuthor")]
        public async Task<IActionResult> UpdateAuthor([FromBody] RequestModel request)
        {
            try
            {
                var response = await _masterService.UpdateAuthorAsync(request);
                if (response == null || response.StatusCode != 0) return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = response?.Message ?? "Vui lòng liên hệ IT để được hổ trợ."
                });
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MasterDataController", "UpdateAuthor");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });

            }
        }
    }
}
