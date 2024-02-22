using Blazored.LocalStorage;
using LM.Models;
using LM.WEB.Commons;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;

namespace LM.WEB.Services
{
    public interface ICliDocumentService
    {
        Task<bool> UpdateBorrowOrder(string pJson, string pJsonDetail, string pAction, int pUserId);
        Task<List<BorrowOrderModel>?> GetBorrowOrdersAsync(SearchModel pSearch);

        Task<Dictionary<string, string>?> GetDocByIdAsync(string pVoucherNo);
        Task<bool> ReturnBooksAsync(string pJson, string pJsonDetail, string pAction, int pUserId);
        Task<Dictionary<string, int>?> GetReportIndexAsync();
        Task<bool> CancleDocList(string pJsonIds, string pReasonDelete, int pUserId, string pTableName);
        Task<List<BorrowOrderModel>?> GetDocumentByStaffAsync(string pStaffCode);
    }
    public class CliDocumentService : CliServiceBase, ICliDocumentService
    {
        private readonly ToastService _toastService;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public CliDocumentService(IHttpClientFactory factory, ILogger<CliMasterDataService> logger
        , ToastService toastService, ILocalStorageService localStorage, AuthenticationStateProvider authenticationStateProvider)
        : base(factory, logger)
        {
            _toastService = toastService;
            _localStorage = localStorage;
            _authenticationStateProvider = authenticationStateProvider;
        }


        /// <summary>
        /// cập nhật thông tin SalesOrder
        /// </summary>
        /// <param name="pJson"></param>
        /// <param name="pAction"></param>
        /// <param name="pUserId"></param>
        /// <returns></returns>
        public async Task<bool> UpdateBorrowOrder(string pJson, string pJsonDetail, string pAction, int pUserId)
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
                HttpResponseMessage httpResponse = await PostAsync(EndpointConstants.URL_DOCUMENT_UPDATE_BORROW_ORDER, request);
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
                        _toastService.ShowSuccess($"{sMessage} thông tin phiếu mượn!");
                        return true;
                    }
                    _toastService.ShowError($"{oResponse.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateSalesOrder");
                _toastService.ShowError(ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Call API lấy danh sách Vị trí sách
        /// </summary>
        /// <returns></returns>
        public async Task<List<BorrowOrderModel>?> GetBorrowOrdersAsync(SearchModel pSearch)
        {
            try
            {
                HttpResponseMessage httpResponse = await PostAsync(EndpointConstants.URL_DOCUMENT_GET_BORROW_ORDER, pSearch);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
                else
                {
                    var content = await httpResponse.Content.ReadAsStringAsync();
                    if (httpResponse.IsSuccessStatusCode) return JsonConvert.DeserializeObject<List<BorrowOrderModel>>(content);
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
        /// Call API lấy danh sách phiếu mượn
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, string>?> GetDocByIdAsync(string pVoucherNo)
        {
            try
            {
                Dictionary<string, object?> pParams = new Dictionary<string, object?>()
            {
                {"pVoucherNo", $"{pVoucherNo}"}
            };
                HttpResponseMessage httpResponse = await GetAsync(EndpointConstants.URL_DOCUMENT_GET_DOC_BY_ID, pParams);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
                else
                {
                    var content = await httpResponse.Content.ReadAsStringAsync();
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        var dt = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
                        if (dt == null || dt.Keys.Count < 1) _toastService.ShowWarning(DefaultConstants.MESSAGE_NO_DATA);
                        return dt;

                    }
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
                _logger.LogError(ex, "GetDocByIdAsync");
                _toastService.ShowError(ex.Message);
            }
            return default;
        }

        /// <summary>
        /// cập nhật thông tin SalesOrder
        /// </summary>
        /// <param name="pJson"></param>
        /// <param name="pAction"></param>
        /// <param name="pUserId"></param>
        /// <returns></returns>
        public async Task<bool> ReturnBooksAsync(string pJson, string pJsonDetail, string pAction, int pUserId)
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
                HttpResponseMessage httpResponse = await PostAsync(EndpointConstants.URL_DOCUMENT_GET_DOC_RETURN_BOOK, request);
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
                        _toastService.ShowSuccess($"Đã trả sách thành công!");
                        return true;
                    }
                    _toastService.ShowError($"{oResponse.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateSalesOrder");
                _toastService.ShowError(ex.Message);
            }
            return false;
        }


        /// <summary>
        /// gọi API lấy báo cáo trên trang Index
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, int>?> GetReportIndexAsync()
        {
            try
            {
                HttpResponseMessage httpResponse = await GetAsync(EndpointConstants.URL_DOCUMENT_GET_DOC_REPORT_INDEX);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
                else
                {
                    var content = await httpResponse.Content.ReadAsStringAsync();
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        var dt = JsonConvert.DeserializeObject<Dictionary<string, int>>(content);
                        if (dt == null || dt.Keys.Count < 1) _toastService.ShowWarning(DefaultConstants.MESSAGE_NO_DATA);
                        return dt;

                    }
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
                _logger.LogError(ex, "GetReportIndexAsync");
                _toastService.ShowError(ex.Message);
            }
            return default;
        }

        /// <summary>
        /// Call API hủy phiếu mượn
        /// </summary>
        /// <param name="pJson"></param>
        /// <param name="pAction"></param>
        /// <param name="pUserId"></param>
        /// <returns></returns>
        public async Task<bool> CancleDocList(string pJsonIds, string pReasonDelete, int pUserId, string pTableName)
        {
            try
            {
                RequestModel request = new RequestModel
                {
                    Json = pJsonIds,
                    JsonDetail = pReasonDelete,
                    UserId = pUserId,
                    Type = pTableName
                };
                //var savedToken = await _localStorage.GetItemAsync<string>("authToken");
                //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", savedToken);
                HttpResponseMessage httpResponse = await PostAsync(EndpointConstants.URL_DOCUMENT_CANCLE_DOC_LIST, request);
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
                        _toastService.ShowSuccess($"Đã hủy danh sách phiếu mượn!");
                        return true;
                    }
                    _toastService.ShowError($"{oResponse.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CancleDocList");
                _toastService.ShowError(ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Call API lấy danh sách phiếu mượn theo phân trang
        /// </summary>
        /// <returns></returns>
        public async Task<List<BorrowOrderModel>?> GetDocumentByStaffAsync(string pStaffCode)
        {
            try
            {
                Dictionary<string, object?> pParams = new Dictionary<string, object?>()
                {
                    {"pStaffCode", $"{pStaffCode}"}
                };
                HttpResponseMessage httpResponse = await GetAsync(EndpointConstants.URL_DOCUMENT_GET_BORROW_ORDER_BY_STAFF, pParams);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowError(DefaultConstants.MESSAGE_INVALID_DATA);
                else
                {
                    var content = await httpResponse.Content.ReadAsStringAsync();
                    if (httpResponse.IsSuccessStatusCode) return JsonConvert.DeserializeObject<List<BorrowOrderModel>>(content);
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
                _logger.LogError(ex, "GetDocByIdAsync");
                _toastService.ShowError(ex.Message);
            }
            return default;
        }

    }
}
