﻿using Blazored.LocalStorage;
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
    }
}
