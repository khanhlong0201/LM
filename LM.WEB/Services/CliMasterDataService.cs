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
}
public class CliMasterDataService : CliServiceBase, ICliMasterDataService 
{
    private readonly ToastService _toastService;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    public CliMasterDataService(IHttpClientFactory factory, ILogger<CliMasterDataService> logger
        , ToastService toastService, ILocalStorageService localStorage, AuthenticationStateProvider authenticationStateProvider)
        : base(factory, logger)
    {
        _toastService = toastService;
        _localStorage = localStorage;
        _authenticationStateProvider = authenticationStateProvider;
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
    public async Task<List<PublisherModel>?> GetDataBooksAsync()
    {
        try
        {
            HttpResponseMessage httpResponse = await GetAsync(EndpointConstants.URL_MASTERDATA_GET_BOOK);
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
}
