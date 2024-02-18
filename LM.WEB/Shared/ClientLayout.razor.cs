using Blazored.LocalStorage;
using LM.Models;
using LM.Models.Shared;
using LM.WEB.Components;
using LM.WEB.Models;
using LM.WEB.Services;
using LM.WEB.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Telerik.Blazor;
using Telerik.Blazor.Components;

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
        [Inject] private ICliDocumentService? _documentService { get; init; }

        public string FullName { get; set; } = "";
        public string StaffCode { get; set; } = "";
        public string Department { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Email { get; set; } = "";
        public bool IsLogin { get; set; }
        public bool IsShowDialog { get; set; }
        int QtyBO = 0;
        public List<BookModel>? ListBooks { get; set; }
        public TelerikGrid<BookModel>? RefListBooks { get; set; }

        [CascadingParameter]
        public DialogFactory? _rDialogs { get; set; }

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
                    Department = oData.Department + "";
                    PhoneNumber = oData.PhoneNumber + "";
                    Email = oData.Email + "";
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
            var checkExists = ListBooks.FirstOrDefault(m => m.BookId == _lstBooks.BookId);
            if(checkExists != null)
            {
                checkExists.QtyBO++;
            }    
            else
            {
                _lstBooks.QtyBO = 1;
                ListBooks.Add(_lstBooks);
            }
            QtyBO++;
        }

        /// <summary>
        /// loading
        /// </summary>
        /// <param name="isShow"></param>
        /// <returns></returns>
        public async Task ShowLoader(bool isShow = true)
        {
            if (isShow)
            {
                _loaderService!.ShowLoader(isShow);
                await Task.Yield();
                return;
            }
            _loaderService!.ShowLoader(isShow);
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
                if (IsLogin)
                {
                    IsShowDialog = true;
                    StateHasChanged();
                } 
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

        protected void RemoveBook(BookModel? oBook)
        {
            try
            {
                if(oBook != null && ListBooks != null)
                {
                    ListBooks.Remove(oBook);
                    QtyBO = QtyBO - oBook.QtyBO;
                    RefListBooks?.Rebind();
                    StateHasChanged();
                }    

            }
            catch (Exception ex)
            {
                _toastService!.ShowError(ex.Message);
            }
        }    


        protected async Task SaveDocHandler()
        {
            try
            {
                if(ListBooks == null || !ListBooks.Any())
                {
                    _toastService!.ShowWarning("Vui lòng thêm sách cần mượn vào giỏ hàng!!!");
                    return;
                }    
                if (!IsLogin) return;

                // Kiểm tra đơn hàng chờ duyệt -> không cho đặt tiếp
                bool isConfirm = await _rDialogs!.ConfirmAsync($"Bạn có chắc muốn Lưu thông tin phiếu mượn này ?", "Thông báo");
                if (!isConfirm) return;
                // thêm 
                await ShowLoader();
                BorrowOrderModel oHeader = new BorrowOrderModel();
                oHeader.StaffCode = StaffCode;
                oHeader.StatusCode = nameof(DocStatus.ApprovalPending);
                oHeader.DocDate = DateTime.Now;
                oHeader.TypeBO = "Online";
                oHeader.Description = "Qui trình mượn sách Online";

                List<BODetailModel> lstBODetails = new List<BODetailModel>();
                foreach (var book in ListBooks)
                {
                    BODetailModel oItem = new BODetailModel();
                    oItem.BookSerialId = -1; // default một số Serial không xác định
                    oItem.StatusCode = nameof(DocStatus.Pending);
                    oItem.NoteForAll = "";
                    oItem.Quantity = book.QtyBO;
                    oItem.BookId = book.BookId;
                    lstBODetails.Add(oItem);
                }
                bool isSuccess = await _documentService!.UpdateBorrowOrder(JsonConvert.SerializeObject(oHeader)
                    , JsonConvert.SerializeObject(lstBODetails), nameof(EnumType.Add), -1);

                if(isSuccess)
                {
                    //
                    //_toastService!.ShowSuccess()
                    ListBooks = new List<BookModel>();
                    RefListBooks?.Rebind();
                    QtyBO = 0;
                    IsShowDialog = false;
                }    
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "DocumentController", "SaveDocHandler");
                _toastService!.ShowError(ex.Message);
            }
            finally
            {
                await Task.Delay(100);
                await ShowLoader(false);
                await InvokeAsync(StateHasChanged);
            }
        }    
        #endregion
    }


}
