using Blazored.LocalStorage;
using LM.Models;
using LM.Models.Shared;
using LM.WEB.Models;
using LM.WEB.Services;
using LM.WEB.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace LM.WEB.Shared
{
    public partial class ClientLayout 
    {
        [Inject] AuthenticationStateProvider? _authenticationStateProvider { get; set; }
        [Inject] private ILogger<ClientLayout>? _logger { get; init; }
        [Inject] private ICliMasterDataService? _masterDataService { get; init; }
        [Inject] private IConfiguration? _configuration { get; set; }
        [Inject] NavigationManager? _navigationManager { get; set; }
        [Inject] public LoaderService? _loaderService { get; init; }
        [Inject] private ToastService? _toastService { get; init; }
        [Inject] private ILocalStorageService? _localStorage{ get; init; }
        [Inject] private LoginDialogService? _bhDialogService { get; init; }

        public string FullName { get; set; } = "";
        public string StaffCode { get; set; } = "";
        public string StaffType { get; set; } = "";
        public bool IsLogin { get; set; }

        public List<BookModel>? ListBooks { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            try
            {
                var oData = await _localStorage!.GetItemAsync<LoginResponseViewModel>("authCliToken");
                if (oData != null)
                {
                    IsLogin = true;
                    FullName = oData.FullName + "";
                    StaffCode = oData.StaffCode + "";
                    StaffType = oData.StaffTypeName + "";
                }
            }
            catch (Exception) { }
        }

        #region "Private Functions"
        EventCallback<BookModel> BooksHandler =>
        EventCallback.Factory.Create(this, (Action<BookModel>)NotifyBooks);
        private void NotifyBooks(BookModel _lstBooks)
        {
            if (ListBooks == null) ListBooks = new List<BookModel>();
            ListBooks.Add(_lstBooks);
        }

        #endregion

        #region
        protected async void CliLogoutAsync()
        {
            try
            {
                IsLogin = false;
                await _localStorage!.RemoveItemAsync("authCliToken");
                _navigationManager!.NavigateTo("trang-chu", true);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                _toastService!.ShowError(ex.Message);
            }
        }    

        protected void LoginAsync()
        {
            try
            {
                _bhDialogService!.ShowDialog();
            }
            catch (Exception ex)
            {
                _toastService!.ShowError(ex.Message);
            }
        }

        protected void GoToCart()
        {
            try
            {
                if(IsLogin) _navigationManager!.NavigateTo("cart");
                else
                {
                    _toastService!.ShowInfo("Vui lòng đăng nhập");
                }    
            }
            catch (Exception ex)
            {
                _toastService!.ShowError(ex.Message);
            }
        }    
        #endregion
    }


}
