using Blazored.LocalStorage;
using LM.Models;
using LM.WEB.Commons;
using LM.WEB.Models;
using LM.WEB.Providers;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
using static System.Net.Mime.MediaTypeNames;
using System.Text;
using Microsoft.AspNetCore.Components.Forms;
using System.Text.Json;

namespace LM.WEB.Services;

public interface ICliMasterDataService
{
    Task<List<UserModel>?> GetDataUsersAsync(int pUserId = -1);
    Task<bool> UpdateUserAsync(string pJson, string pAction, int pUserId);
    Task<List<KindBookModel>?> GetDataKindBooksAsync();
    Task<bool> UpdateKindBookAsync(string pJson, string pAction, int pUserId);
    Task<string> LoginAsync(LoginViewModel request);
    Task LogoutAsync();
    Task<bool> DeleteDataAsync(string pTableName, string pReasonDelete, string pValue, int pUserId);
    Task<List<PublisherModel>?> GetDataPublishersAsync();
    Task<bool> UpdatePublisherAsync(string pJson, string pAction, int pUserId);
    Task<List<BookModel>?> GetDataBooksAsync(SearchModel pSearch);
    Task<bool> UpdateBookAsync(string pJson, string pAction, int pUserId);
    Task<string> UploadMultiFiles(string link, List<IBrowserFile> lstIBrowserFiles);
    Task<List<ImageDetailModel>?> GetDataImageDetailsAsync(int imageId);
    Task<List<ReaderModel>?> GetDataReadersAsync();
    Task<List<BatchModel>?> GetDataBatchsAsync(int batchId);
    Task<bool> UpdateBatchAsync(string pJson, string pAction, int pUserId);
    Task<List<SeriesModel>?> GetDataSeriesAsync(int batchId);
    Task<bool> UpdateSeriesAsync(string pJson, string pJsonDetail, string pAction, int pUserId);
    Task<List<CliBookModel>?> GetDataBookClientsAsync(SearchModel pSearch);
    Task<BorrowingModel> GetDataBookDetailClientsAsync(int bookId);
    Task<List<LocationModel>?> GetLocationsAsync();
    Task<bool> UpdateLocationAsync(string pJson, string pAction, int pUserId);

    Task<List<AuthorModel>?> GetAuthorsAsync();
    Task<bool> UpdateAuthorAsync(string pJson, string pAction, int pUserId);

    Task<List<ImageModel>?> UploadImagesAsync(List<ImageModel> pListImgs);
    Task<List<StaffModel>?> GetStaffsAsync();
}
public class CliMasterDataService : CliServiceBase, ICliMasterDataService 
{
    private readonly ToastService _toastService;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IWebHostEnvironment _webHostEnvironment;
    long maxFileSize = 134217728;
    public CliMasterDataService(IHttpClientFactory factory, ILogger<CliMasterDataService> logger
        , ToastService toastService, ILocalStorageService localStorage, AuthenticationStateProvider authenticationStateProvider, IWebHostEnvironment webHostEnvironment)
        : base(factory, logger)
    {
        _toastService = toastService;
        _localStorage = localStorage;
        _authenticationStateProvider = authenticationStateProvider;
        _webHostEnvironment = webHostEnvironment;
    }

    /// <summary>
    /// Call API lấy danh sách user
    /// </summary>
    /// <returns></returns>
    public async Task<List<UserModel>?> GetDataUsersAsync(int pUserId = -1)
    {
        try
        {
            Dictionary<string, object?> pParams = new Dictionary<string, object?>()
            {
                {"pUserId", $"{pUserId}"}
            };
            HttpResponseMessage httpResponse = await GetAsync(EndpointConstants.URL_MASTERDATA_GET_USER, pParams);
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.IsSuccessStatusCode) return JsonConvert.DeserializeObject<List<UserModel>>(content);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                    return null;
                }
                var oMessage = JsonConvert.DeserializeObject<ResponseModel>(content);
                _toastService.ShowError($"{oMessage?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDataBranchsAsync");
            _toastService.ShowError(ex.Message);
        }
        return default;
    }

    /// <summary>
    /// cập nhật nhân viên
    /// </summary>
    /// <param name="pJson"></param>
    /// <param name="pAction"></param>
    /// <param name="pUserId"></param>
    /// <returns></returns>
    public async Task<bool> UpdateUserAsync(string pJson, string pAction, int pUserId)
    {
        try
        {
            RequestModel request = new RequestModel
            {
                Json = pJson,
                Type = pAction,
                UserId = pUserId
            };
            //var savedToken = await _localStorage.GetItemAsync<string>("authToken");
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", savedToken);
            HttpResponseMessage httpResponse = await PostAsync(EndpointConstants.URL_MASTERDATA_UPDATE_USER, request);
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                return false;
            }
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                ResponseModel oResponse = JsonConvert.DeserializeObject<ResponseModel>(content)!;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string sMessage = pAction == nameof(EnumType.Add) ? DefaultConstants.MESSAGE_INSERT : DefaultConstants.MESSAGE_UPDATE;
                    
                    if(pAction == nameof(EnumType.ChangePassWord)) _toastService.ShowSuccess($"{sMessage} Mật khẩu!");
                    else _toastService.ShowSuccess($"{sMessage} Nhân viên!");
                    return true;
                }
                _toastService.ShowError($"{oResponse.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateBranchAsync");
            _toastService.ShowError(ex.Message);
        }
        return false;
    }
    public async Task<string> LoginAsync(LoginViewModel request)
    {
        try
        {
            var loginRequest = new LoginViewModel();
            loginRequest.UserName = request.UserName;
            loginRequest.Password = LM.Models.Shared.EncryptHelper.Encrypt(request.Password + "");
            string jsonBody = JsonConvert.SerializeObject(loginRequest);
            HttpResponseMessage httpResponse = await _httpClient.PostAsync($"api/{EndpointConstants.URL_MASTERDATA_USER_LOGIN}", new StringContent(jsonBody, UnicodeEncoding.UTF8, "application/json"));
            Debug.Print(jsonBody);
            var content = await httpResponse.Content.ReadAsStringAsync();
            LoginResponseViewModel response = JsonConvert.DeserializeObject<LoginResponseViewModel>(content)!;
            if (!httpResponse.IsSuccessStatusCode) return response.Message + "";
            // save token
            if (await _localStorage.ContainKeyAsync("authToken")) await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.SetItemAsync("authToken", response.Token);
            ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated($"{response!.FullName}");
            _httpClient.DefaultRequestHeaders.Add("UserId", $"{response!.UserId}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", response!.Token);
            return "";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login");
            return ex.Message;
        }
    }
    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
    }

    /// <summary>
    /// cập nhật thông tin dịch vụ
    /// </summary>
    /// <param name="pJson"></param>
    /// <param name="pAction"></param>
    /// <param name="pUserId"></param>
    /// <returns></returns>
    public async Task<bool> DeleteDataAsync(string pTableName, string pReasonDelete, string pValue, int pUserId)
    {
        try
        {
            RequestModel request = new RequestModel
            {
                Json = pValue,
                Type = pTableName,
                JsonDetail = pReasonDelete,
                UserId = pUserId
            };
            //var savedToken = await _localStorage.GetItemAsync<string>("authToken");
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", savedToken);
            HttpResponseMessage httpResponse = await PostAsync(EndpointConstants.URL_MASTERDATA_DELETE, request);
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                return false;
            }
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                ResponseModel oResponse = JsonConvert.DeserializeObject<ResponseModel>(content)!;
                if (httpResponse.IsSuccessStatusCode)
                {
                    _toastService.ShowSuccess(DefaultConstants.MESSAGE_DELETE);
                    return true;
                }
                _toastService.ShowError($"{oResponse.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteDataAsync");
            _toastService.ShowError(ex.Message);
        }
        return false;
    }

    /// <summary>
    /// Call API lấy danh sách loại sách
    /// </summary>
    /// <returns></returns>
    public async Task<List<KindBookModel>?> GetDataKindBooksAsync()
    {
        try
        {
            HttpResponseMessage httpResponse = await GetAsync(EndpointConstants.URL_MASTERDATA_GET_KINDBOOK);
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.IsSuccessStatusCode) return JsonConvert.DeserializeObject<List<KindBookModel>>(content);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                    return null;
                }
                var oMessage = JsonConvert.DeserializeObject<ResponseModel>(content);
                _toastService.ShowError($"{oMessage?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDataBranchsAsync");
            _toastService.ShowError(ex.Message);
        }
        return default;
    }

    /// <summary>
    /// cập nhật loại sách
    /// </summary>
    /// <param name="pJson"></param>
    /// <param name="pAction"></param>
    /// <param name="pUserId"></param>
    /// <returns></returns>
    public async Task<bool> UpdateKindBookAsync(string pJson, string pAction, int pUserId)
    {
        try
        {
            RequestModel request = new RequestModel
            {
                Json = pJson,
                Type = pAction,
                UserId = pUserId
            };
            //var savedToken = await _localStorage.GetItemAsync<string>("authToken");
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", savedToken);
            HttpResponseMessage httpResponse = await PostAsync(EndpointConstants.URL_MASTERDATA_UPDATE_KINDBOOK, request);
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                return false;
            }
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                ResponseModel oResponse = JsonConvert.DeserializeObject<ResponseModel>(content)!;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string sMessage = pAction == nameof(EnumType.Add) ? DefaultConstants.MESSAGE_INSERT : DefaultConstants.MESSAGE_UPDATE;

                    if (pAction == nameof(EnumType.ChangePassWord)) _toastService.ShowSuccess($"{sMessage} Mật khẩu!");
                    else _toastService.ShowSuccess($"{sMessage} Loại sách!");
                    return true;
                }
                _toastService.ShowError($"{oResponse.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateKindBookAsync");
            _toastService.ShowError(ex.Message);
        }
        return false;
    }

    /// <summary>
    /// Call API lấy danh sách nhà xuất bản
    /// </summary>
    /// <returns></returns>
    public async Task<List<PublisherModel>?> GetDataPublishersAsync()
    {
        try
        {
            HttpResponseMessage httpResponse = await GetAsync(EndpointConstants.URL_MASTERDATA_GET_PUBLISHER);
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.IsSuccessStatusCode) return JsonConvert.DeserializeObject<List<PublisherModel>>(content);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                    return null;
                }
                var oMessage = JsonConvert.DeserializeObject<ResponseModel>(content);
                _toastService.ShowError($"{oMessage?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDataPubshersAsync");
            _toastService.ShowError(ex.Message);
        }
        return default;
    }

    /// <summary>
    /// cập nhật nhà xuất bản
    /// </summary>
    /// <param name="pJson"></param>
    /// <param name="pAction"></param>
    /// <param name="pUserId"></param>
    /// <returns></returns>
    public async Task<bool> UpdatePublisherAsync(string pJson, string pAction, int pUserId)
    {
        try
        {
            RequestModel request = new RequestModel
            {
                Json = pJson,
                Type = pAction,
                UserId = pUserId
            };
            //var savedToken = await _localStorage.GetItemAsync<string>("authToken");
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", savedToken);
            HttpResponseMessage httpResponse = await PostAsync(EndpointConstants.URL_MASTERDATA_UPDATE_PUBLISHER, request);
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                return false;
            }
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                ResponseModel oResponse = JsonConvert.DeserializeObject<ResponseModel>(content)!;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string sMessage = pAction == nameof(EnumType.Add) ? DefaultConstants.MESSAGE_INSERT : DefaultConstants.MESSAGE_UPDATE;
                    _toastService.ShowSuccess($"{sMessage} Nhà xuất bản!");
                    return true;
                }
                _toastService.ShowError($"{oResponse.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdatePublisherAsync");
            _toastService.ShowError(ex.Message);
        }
        return false;
    }

    /// <summary>
    /// Call API lấy danh sách sách
    /// </summary>
    /// <returns></returns>
    public async Task<List<BookModel>?> GetDataBooksAsync(SearchModel pSearch)
    {
        try
        {
            HttpResponseMessage httpResponse = await PostAsync(EndpointConstants.URL_MASTERDATA_GET_BOOK, pSearch);
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.IsSuccessStatusCode) return JsonConvert.DeserializeObject<List<BookModel>>(content);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                    return null;
                }
                var oMessage = JsonConvert.DeserializeObject<ResponseModel>(content);
                _toastService.ShowError($"{oMessage?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDataBooksAsync");
            _toastService.ShowError(ex.Message);
        }
        return default;
    }

    /// <summary>
    /// cập nhật sách
    /// </summary>
    /// <param name="pJson"></param>
    /// <param name="pAction"></param>
    /// <param name="pUserId"></param>
    /// <returns></returns>
    public async Task<bool> UpdateBookAsync(string pJson, string pAction, int pUserId)
    {
        try
        {
            RequestModel request = new RequestModel
            {
                Json = pJson,
                Type = pAction,
                UserId = pUserId
            };
            //var savedToken = await _localStorage.GetItemAsync<string>("authToken");
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", savedToken);
            HttpResponseMessage httpResponse = await PostAsync(EndpointConstants.URL_MASTERDATA_UPDATE_BOOK, request);
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                return false;
            }
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                ResponseModel oResponse = JsonConvert.DeserializeObject<ResponseModel>(content)!;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string sMessage = pAction == nameof(EnumType.Add) ? DefaultConstants.MESSAGE_INSERT : DefaultConstants.MESSAGE_UPDATE;
                    _toastService.ShowSuccess($"{sMessage} Sách!");
                    return true;
                }
                _toastService.ShowError($"{oResponse.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateBookAsync");
            _toastService.ShowError(ex.Message);
        }
        return false;
    }

    /// <summary>
    /// Xử lý call api Upload file
    /// </summary>
    /// <param name="link"></param>
    /// <param name="lstIBrowserFiles"></param>
    /// <param name="strSubFolder"></param>
    /// <param name="strSubFolderProdLine"></param>
    /// <returns></returns>
    public async Task<string> UploadMultiFiles(string link, List<IBrowserFile> lstIBrowserFiles,
            string strSubFolder, string strSubFolderProdLine)
    {
        string json = JsonConvert.SerializeObject(lstIBrowserFiles);
        string strResponse = "";
        try
        {
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            using var content = new MultipartFormDataContent();
            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
            foreach (var file in lstIBrowserFiles)
            {
                var fileContent = new StreamContent(file.OpenReadStream(maxFileSize));
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(
                    content: fileContent,
                    name: "\"files\"",
                    fileName: file.Name);
            }
            string URL = $"/api/{link}?strSubFolder={strSubFolder}&strSubFolderProdLine={strSubFolderProdLine}";
            var objResponse = await _httpClient.PostAsync(URL, content);
            if (objResponse.IsSuccessStatusCode)
            {
                // cho phép properties name trùng, phân biệt hoa thường
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                strResponse = await objResponse.Content.ReadAsStringAsync();
            }

            Debug.Print(_httpClient.BaseAddress + URL);
            Debug.Print(json);
            return strResponse;
        }
        catch (Exception objEx)
        {
            _logger.LogError(objEx, json);
            return JsonConvert.SerializeObject(new ResponseModel<object> { StatusCode = -1, Message = objEx.Message });
        }
    }

    /// <summary>
    /// upload multifile
    /// </summary>
    /// <param name="link"></param>
    /// <param name="lstIBrowserFiles"></param>
    /// <returns></returns>
    public async Task<string> UploadMultiFiles(string link, List<IBrowserFile> lstIBrowserFiles)
    {
        List<string> listFilePath = new List<string>();
        try
        {
            Console.WriteLine(System.Net.ServicePointManager.DefaultConnectionLimit);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            using var content = new MultipartFormDataContent();
            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
            var rootFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Upload", "TEMP");
            //tạo thư mục
            if (!Directory.Exists(rootFolder)) Directory.CreateDirectory(rootFolder);
            string strFileFullName = string.Empty;
            foreach (var file in lstIBrowserFiles)
            {
                string fileNameNew = $"{Guid.NewGuid()}---{file.Name}";
                strFileFullName = Path.Combine(rootFolder, fileNameNew);
                await using FileStream fs = new(strFileFullName, FileMode.Create);
                await file.OpenReadStream(long.MaxValue).CopyToAsync(fs); // lưu vào www tạm để đọc file
                listFilePath.Add(strFileFullName); // khi lưu vào www ok -> lưu vào list tạm để finally -> remove ra
                await fs.FlushAsync();
                await fs.DisposeAsync();
                content.Add(new StreamContent(File.OpenRead(@$"{strFileFullName}")), name: "\"files\"", fileName: fileNameNew);
            }

            string sUrl = $"api/{link}";
            HttpResponseMessage httpResponse = await _httpClient.PostAsync(sUrl, content);
            var resContent = await httpResponse.Content.ReadAsStringAsync();
            if (httpResponse.IsSuccessStatusCode) return resContent; // nếu APi trả về OK 200
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                //_bhDialogService.ShowDialog();
                _toastService.ShowInfo("Hết phiên đăng nhập!");
                return "";
            }
            var oMessage = JsonConvert.DeserializeObject<ResponseModel>(resContent); // mã lỗi dưới API
            _toastService.ShowError($"{oMessage?.Message}");
            return "";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UploadMultiFiles");
            _toastService.ShowError(ex.Message);
            return "";
        }
        finally
        {
            // xóa các ảnh được lưu vào folder tạm
            listFilePath.ForEach(item => { if (File.Exists(@$"{item}")) File.Delete(item); });
        }
    }

    /// <summary>
    /// Call API lấy danh sách sách
    /// </summary>
    /// <returns></returns>
    public async Task<List<ImageDetailModel>?> GetDataImageDetailsAsync(int imageId = -1)
    {
        try
        {
            Dictionary<string, object> pParams = new Dictionary<string, object>()
            {
                {"ImageId", $"{imageId}"}
            };
            HttpResponseMessage httpResponse = await GetAsync(EndpointConstants.URL_MASTERDATA_GET_IMAGE_DETAILS, pParams);
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.IsSuccessStatusCode) return JsonConvert.DeserializeObject<List<ImageDetailModel>>(content);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                    return null;
                }
                var oMessage = JsonConvert.DeserializeObject<ResponseModel>(content);
                _toastService.ShowError($"{oMessage?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDataImageDetailsAsync");
            _toastService.ShowError(ex.Message);
        }
        return default;
    }

    /// <summary>
    /// Call API lấy danh sách sách đọc giả
    /// </summary>
    /// <returns></returns>
    public async Task<List<ReaderModel>?> GetDataReadersAsync()
    {
        try
        {
            HttpResponseMessage httpResponse = await GetAsync(EndpointConstants.URL_MASTERDATA_GET_READER);
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.IsSuccessStatusCode) return JsonConvert.DeserializeObject<List<ReaderModel>>(content);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                    return null;
                }
                var oMessage = JsonConvert.DeserializeObject<ResponseModel>(content);
                _toastService.ShowError($"{oMessage?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDataBooksAsync");
            _toastService.ShowError(ex.Message);
        }
        return default;
    }

    /// <summary>
    /// Call API lấy danh lô sách
    /// </summary>
    /// <returns></returns>
    public async Task<List<BatchModel>?> GetDataBatchsAsync(int bookId)
    {
        try
        {
            Dictionary<string, object> pParams = new Dictionary<string, object>()
            {
                {"BookId", $"{bookId}"}
            };
            HttpResponseMessage httpResponse = await GetAsync(EndpointConstants.URL_MASTERDATA_GET_BATCH,pParams);
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.IsSuccessStatusCode) return JsonConvert.DeserializeObject<List<BatchModel>>(content);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                    return null;
                }
                var oMessage = JsonConvert.DeserializeObject<ResponseModel>(content);
                _toastService.ShowError($"{oMessage?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDataBatchsAsync");
            _toastService.ShowError(ex.Message);
        }
        return default;
    }

    /// <summary>
    /// cập nhật lô sách
    /// </summary>
    /// <param name="pJson"></param>
    /// <param name="pAction"></param>
    /// <param name="pUserId"></param>
    /// <returns></returns>
    public async Task<bool> UpdateBatchAsync(string pJson, string pAction, int pUserId)
    {
        try
        {
            RequestModel request = new RequestModel
            {
                Json = pJson,
                Type = pAction,
                UserId = pUserId
            };
            //var savedToken = await _localStorage.GetItemAsync<string>("authToken");
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", savedToken);
            HttpResponseMessage httpResponse = await PostAsync(EndpointConstants.URL_MASTERDATA_UPDATE_BATCH, request);
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                return false;
            }
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                ResponseModel oResponse = JsonConvert.DeserializeObject<ResponseModel>(content)!;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string sMessage = pAction == nameof(EnumType.Add) ? DefaultConstants.MESSAGE_INSERT : DefaultConstants.MESSAGE_UPDATE;
                    _toastService.ShowSuccess($"{sMessage} Sách!");
                    return true;
                }
                _toastService.ShowError($"{oResponse.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateBatchAsync");
            _toastService.ShowError(ex.Message);
        }
        return false;
    }

    /// <summary>
    /// Call API lấy danh seri của sách
    /// </summary>
    /// <returns></returns>
    public async Task<List<SeriesModel>?> GetDataSeriesAsync(int batchId)
    {
        try
        {
            Dictionary<string, object> pParams = new Dictionary<string, object>()
            {
                {"BatchId", $"{batchId}"}
            };
            HttpResponseMessage httpResponse = await GetAsync(EndpointConstants.URL_MASTERDATA_GET_SERI, pParams);
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.IsSuccessStatusCode) return JsonConvert.DeserializeObject<List<SeriesModel>>(content);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                    return null;
                }
                var oMessage = JsonConvert.DeserializeObject<ResponseModel>(content);
                _toastService.ShowError($"{oMessage?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDataSeriesAsync");
            _toastService.ShowError(ex.Message);
        }
        return default;
    }

    /// <summary>
    /// cập nhật só seri sách
    /// </summary>
    /// <param name="pJson"></param>
    /// <param name="pAction"></param>
    /// <param name="pUserId"></param>
    /// <returns></returns>
    public async Task<bool> UpdateSeriesAsync(string pJson,string pJsonDetail, string pAction, int pUserId)
    {
        try
        {
            RequestModel request = new RequestModel
            {
                Json = pJson,
                JsonDetail = pJsonDetail,
                Type = pAction,
                UserId = pUserId
            };
            //var savedToken = await _localStorage.GetItemAsync<string>("authToken");
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", savedToken);
            HttpResponseMessage httpResponse = await PostAsync(EndpointConstants.URL_MASTERDATA_UPDATE_SERI, request);
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                return false;
            }
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                ResponseModel oResponse = JsonConvert.DeserializeObject<ResponseModel>(content)!;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string sMessage = pAction == nameof(EnumType.Add) ? DefaultConstants.MESSAGE_INSERT : DefaultConstants.MESSAGE_UPDATE;
                    _toastService.ShowSuccess($"{sMessage} SERI!");
                    return true;
                }
                _toastService.ShowError($"{oResponse.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateBatchAsync");
            _toastService.ShowError(ex.Message);
        }
        return false;
    }

    /// <summary>
    /// Call API lấy danh sách sách hiển thị ở client
    /// </summary>
    /// <returns></returns>
    public async Task<List<CliBookModel>?> GetDataBookClientsAsync(SearchModel pSearch)
    {
        try
        {
            HttpResponseMessage httpResponse = await PostAsync(EndpointConstants.URL_MASTERDATA_GET_BOOK_CLIENT, pSearch);
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.IsSuccessStatusCode) return JsonConvert.DeserializeObject<List<CliBookModel>>(content);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                    return null;
                }
                var oMessage = JsonConvert.DeserializeObject<ResponseModel>(content);
                _toastService.ShowError($"{oMessage?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDataBooksAsync");
            _toastService.ShowError(ex.Message);
        }
        return default;
    }

    /// <summary>
    /// Call API lấy danh seri chi tiết sach
    /// </summary>
    /// <returns></returns>
    public async Task<BorrowingModel> GetDataBookDetailClientsAsync(int bookId)
    {
        try
        {
            Dictionary<string, object> pParams = new Dictionary<string, object>()
            {
                {"bookId", $"{bookId}"}
            };
            HttpResponseMessage httpResponse = await GetAsync(EndpointConstants.URL_MASTERDATA_GET_BOOK_DETAIL_CLIENT, pParams);
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.IsSuccessStatusCode) return JsonConvert.DeserializeObject<BorrowingModel>(content);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                    return null;
                }
                var oMessage = JsonConvert.DeserializeObject<ResponseModel>(content);
                _toastService.ShowError($"{oMessage?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDataSeriesAsync");
            _toastService.ShowError(ex.Message);
        }
        return default;
    }

    //

    /// <summary>
    /// Call API lấy danh sách Vị trí sách
    /// </summary>
    /// <returns></returns>
    public async Task<List<LocationModel>?> GetLocationsAsync()
    {
        try
        {
            HttpResponseMessage httpResponse = await GetAsync(EndpointConstants.URL_MASTERDATA_GET_LOCATION);
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.IsSuccessStatusCode) return JsonConvert.DeserializeObject<List<LocationModel>>(content);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                    return null;
                }
                var oMessage = JsonConvert.DeserializeObject<ResponseModel>(content);
                _toastService.ShowError($"{oMessage?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetLocationsAsync");
            _toastService.ShowError(ex.Message);
        }
        return default;
    }

    /// <summary>
    /// cập nhật Vị trí sách
    /// </summary>
    /// <param name="pJson"></param>
    /// <param name="pAction"></param>
    /// <param name="pUserId"></param>
    /// <returns></returns>
    public async Task<bool> UpdateLocationAsync(string pJson, string pAction, int pUserId)
    {
        try
        {
            RequestModel request = new RequestModel
            {
                Json = pJson,
                Type = pAction,
                UserId = pUserId
            };
            //var savedToken = await _localStorage.GetItemAsync<string>("authToken");
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", savedToken);
            HttpResponseMessage httpResponse = await PostAsync(EndpointConstants.URL_MASTERDATA_UPDATE_LOCATION, request);
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                return false;
            }
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                ResponseModel oResponse = JsonConvert.DeserializeObject<ResponseModel>(content)!;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string sMessage = pAction == nameof(EnumType.Add) ? DefaultConstants.MESSAGE_INSERT : DefaultConstants.MESSAGE_UPDATE;
                    _toastService.ShowSuccess($"{sMessage} Vị trí!");
                    return true;
                }
                _toastService.ShowError($"{oResponse.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateLocationAsync");
            _toastService.ShowError(ex.Message);
        }
        return false;
    }

    /// <summary>
    /// Call API lấy danh sách Tác giả
    /// </summary>
    /// <returns></returns>
    public async Task<List<AuthorModel>?> GetAuthorsAsync()
    {
        try
        {
            HttpResponseMessage httpResponse = await GetAsync(EndpointConstants.URL_MASTERDATA_GET_AUTHOR);
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.IsSuccessStatusCode) return JsonConvert.DeserializeObject<List<AuthorModel>>(content);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                    return null;
                }
                var oMessage = JsonConvert.DeserializeObject<ResponseModel>(content);
                _toastService.ShowError($"{oMessage?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetLocationsAsync");
            _toastService.ShowError(ex.Message);
        }
        return default;
    }

    /// <summary>
    /// cập nhật Tác giả
    /// </summary>
    /// <param name="pJson"></param>
    /// <param name="pAction"></param>
    /// <param name="pUserId"></param>
    /// <returns></returns>
    public async Task<bool> UpdateAuthorAsync(string pJson, string pAction, int pUserId)
    {
        try
        {
            RequestModel request = new RequestModel
            {
                Json = pJson,
                Type = pAction,
                UserId = pUserId
            };
            //var savedToken = await _localStorage.GetItemAsync<string>("authToken");
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", savedToken);
            HttpResponseMessage httpResponse = await PostAsync(EndpointConstants.URL_MASTERDATA_UPDATE_AUTHOR, request);
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                return false;
            }
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                ResponseModel oResponse = JsonConvert.DeserializeObject<ResponseModel>(content)!;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string sMessage = pAction == nameof(EnumType.Add) ? DefaultConstants.MESSAGE_INSERT : DefaultConstants.MESSAGE_UPDATE;
                    _toastService.ShowSuccess($"{sMessage} Tác giả!");
                    return true;
                }
                _toastService.ShowError($"{oResponse.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateLocationAsync");
            _toastService.ShowError(ex.Message);
        }
        return false;
    }

    /// <summary>
    /// Upload hình ảnh
    /// </summary>
    /// <param name="pListImgs"></param>
    /// <returns></returns>
    public async Task<List<ImageModel>?> UploadImagesAsync(List<ImageModel> pListImgs)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            using var content = new MultipartFormDataContent();
            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
            if (pListImgs != null && pListImgs.Any())
            {
                var lstUpload = pListImgs.Where(m => m.IsDelete == false); // lấy ra hình ảnh nào cần upload
                foreach (var file in lstUpload)
                {
                    if (File.Exists($"{file.FilePath}"))
                    {
                        content.Add(new StreamContent(File.OpenRead(@$"{file.FilePath}")), name: "\"files\"", fileName: file.FileName + "");
                    }
                }
            }
            HttpResponseMessage httpResponse = await _httpClient.PostAsync($"api/{EndpointConstants.URL_MASTERDATA_UPLOAD_IMAGE}?subFolder={EnumTable.Books}", content);
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var resContent = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.IsSuccessStatusCode) return JsonConvert.DeserializeObject<List<ImageModel>>(resContent);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                    return null;
                }
                var oMessage = JsonConvert.DeserializeObject<ResponseModel>(resContent);
                _toastService.ShowError($"{oMessage?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UploadImagesAsync");
            _toastService.ShowError(ex.Message);
        }
        finally
        {
            // xóa hình ảnh trong folder tạm
            if (pListImgs != null && pListImgs.Any())
            {
                foreach (var file in pListImgs)
                {
                    if (File.Exists($"{file.FilePath}")) File.Delete($"{file.FilePath}");
                }
            }
        }
        return default;

    }

    /// <summary>
    /// Call API lấy danh sách giá viên, cán bộ, sinh viên
    /// </summary>
    /// <returns></returns>
    public async Task<List<StaffModel>?> GetStaffsAsync()
    {
        try
        {
            HttpResponseMessage httpResponse = await GetAsync(EndpointConstants.URL_MASTERDATA_GET_STAFF);
            var checkContent = ValidateJsonContent(httpResponse.Content);
            if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
            else
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.IsSuccessStatusCode) return JsonConvert.DeserializeObject<List<StaffModel>>(content);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _toastService.ShowInfo(DefaultConstants.MESSAGE_LOGIN_EXPIRED);
                    return null;
                }
                var oMessage = JsonConvert.DeserializeObject<ResponseModel>(content);
                _toastService.ShowError($"{oMessage?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetStaffsAsync");
            _toastService.ShowError(ex.Message);
        }
        return default;
    }
}
