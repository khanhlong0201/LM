using LM.Models;
using LM.WEB.Commons;
using LM.WEB.Components;
using LM.WEB.Models;
using LM.WEB.Services;
using LM.WEB.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using Telerik.Blazor.Components;

namespace LM.WEB.Features.Controllers
{
    public class BookSerialController : LMControllerBase
    {
        #region Dependency Injection
        [Inject] private ILogger<BookSerialController>? _logger { get; init; }
        [Inject] private ICliMasterDataService? _masterDataService { get; init; }
        [Inject] private IConfiguration? _configuration { get; set; }
        #endregion

        public bool IsInitialDataLoadComplete { get; set; } = true;
        public List<BookModel>? ListBooks { get; set; }
        public List<BookSerialModel>? ListBookSerials { get; set; }
        public IEnumerable<BookSerialModel> SelectedBookSerials { get; set; } = new List<BookSerialModel>();
        public BookSerialModel BookSerialUpdate { get; set; } = new BookSerialModel();
        public EditContext? _EditContext { get; set; }
        public bool IsShowDialog { get; set; }
        public bool IsCreate { get; set; } = true;
        SearchModel pSearchModel = new SearchModel();
        public HConfirm? _rDialogs { get; set; }

        #region Override Functions
        protected override async Task OnInitializedAsync()
        {
            try
            {
                await base.OnInitializedAsync();
                ListBreadcrumbs = new List<BreadcrumLModel>
                {
                    new BreadcrumLModel() { Text = "Trang chủ", IsShowIcon = true, Icon = "fa-solid fa-house-chimney" },
                    new BreadcrumLModel() { Text = "Thông tin sách" },
                    new BreadcrumLModel() { Text = "Mã sách" }
                };
                await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "OnInitializedAsync");
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                try
                {
                    await _progressService!.SetPercent(0.4);
                    await getBookSerials();
                    
                    ListBooks = await _masterDataService!.GetDataBooksAsync(pSearchModel);
                }
                catch (Exception ex)
                {
                    _logger!.LogError(ex, "OnAfterRenderAsync");
                    ShowError(ex.Message);
                }
                finally
                {
                    await _progressService!.Done();
                    await InvokeAsync(StateHasChanged);
                }
            }
        }
        #endregion

        #region Private Functions
        private async Task getBookSerials()
        {
            ListBookSerials = new List<BookSerialModel>();
            SelectedBookSerials = new List<BookSerialModel>();
            ListBookSerials = await _masterDataService!.GetBookSerialsAsync();
        }
        #endregion

        #region Protected Functions

        protected async void ReLoadDataHandler()
        {
            try
            {
                IsInitialDataLoadComplete = false;
                await getBookSerials();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "BookSerialController", "ReLoadDataHandler");
                ShowError(ex.Message);
            }
            finally
            {
                IsInitialDataLoadComplete = true;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected void OnOpenDialogHandler(EnumType pAction = EnumType.Add, BookSerialModel? pItemDetails = null)
        {
            try
            {
                if (pAction == EnumType.Add)
                {
                    IsCreate = true;
                    BookSerialUpdate = new BookSerialModel();
                }
                else
                {
                    BookSerialUpdate = new BookSerialModel();
                    BookSerialUpdate.Id = pItemDetails!.Id;
                    BookSerialUpdate.BookID = pItemDetails!.BookID;
                    BookSerialUpdate.SerialNumber = pItemDetails!.SerialNumber;
                    BookSerialUpdate.NoteForAll = pItemDetails!.NoteForAll;
                    BookSerialUpdate.BookName = pItemDetails!.BookName;
                    BookSerialUpdate.IsActive = pItemDetails!.IsActive;
                    IsCreate = false;
                }

                _EditContext = new EditContext(BookSerialUpdate);
                IsShowDialog = true;

            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "LocationController", "OnOpenDialogHandler");
                ShowError(ex.Message);
            }
        }

        protected async void SaveDataHandler()
        {
            try
            {
                string sAction = IsCreate ? nameof(EnumType.Add) : nameof(EnumType.Update);
                var checkData = _EditContext!.Validate();
                if (!checkData) return;
                await ShowLoader();
                bool isSuccess = await _masterDataService!.UpdateBookSerialAsync(JsonConvert.SerializeObject(BookSerialUpdate), sAction, pUserId);
                if (isSuccess)
                {
                    await getBookSerials();
                    IsShowDialog = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "BookSerialController", "SaveDataHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await Task.Delay(75);
                await ShowLoader(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        protected void OnRowDoubleClickHandler(GridRowClickEventArgs args) => OnOpenDialogHandler(EnumType.Update, args.Item as BookSerialModel);

        protected async void DeleteDataHandler()
        {
            try
            {
                if (SelectedBookSerials == null || !SelectedBookSerials.Any())
                {
                    ShowWarning(DefaultConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                var confirm = await _rDialogs!.ConfirmAsync($" {DefaultConstants.MESSAGE_CONFIRM_DELETE} ");
                if (!confirm) return;
                await ShowLoader();
                bool isSuccess = await _masterDataService!.DeleteDataAsync(nameof(EnumTable.@BookSerials), "", string.Join(",", SelectedBookSerials.Select(m => m.Id)), pUserId);
                if (isSuccess)
                {
                    await getBookSerials();
                }
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "BookSerialController", "DeleteDataHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await ShowLoader(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        #endregion
    }
}
