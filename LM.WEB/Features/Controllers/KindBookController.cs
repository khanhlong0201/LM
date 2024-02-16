using LM.Models;
using LM.Models.Shared;
using LM.WEB.Commons;
using LM.WEB.Components;
using LM.WEB.Models;
using LM.WEB.Services;
using LM.WEB.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using Telerik.Blazor.Components;

namespace LM.WEB.Features.Controllers;
public class KindBookController : LMControllerBase
{
    #region Dependency Injection
    [Inject] private ILogger<KindBookController>? _logger { get; init; }
    [Inject] private ICliMasterDataService? _masterDataService { get; init; }
    #endregion

    #region Properties
    public bool IsInitialDataLoadComplete { get; set; } = true;
    public List<KindBookModel>? ListKindBooks { get; set; }
    public IEnumerable<KindBookModel>? SelectedKindBooks { get; set; } = new List<KindBookModel>();
    public KindBookModel KindBookUpdate { get; set; } = new KindBookModel();
    public EditContext? _EditContext { get; set; }
    public bool IsShowDialog { get; set; }
    public bool IsCreate { get; set; } = true;
    public HConfirm? _rDialogs { get; set; }
    public List<LocationModel>? ListLocations { get; set; }
    #endregion

    #region Override Functions
    protected override async Task OnInitializedAsync()
    {
        try
        {
            await base.OnInitializedAsync();
            ListBreadcrumbs = new List<BreadcrumLModel>
                {
                    new BreadcrumLModel() { Text = "Trang chủ", IsShowIcon = true, Icon = "fa-solid fa-house-chimney" },
                    new BreadcrumLModel() { Text = "Danh mục" },
                    new BreadcrumLModel() { Text = "Loại sách" }
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
                await getDataKindBooks();
                ListLocations = await _masterDataService!.GetLocationsAsync();
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
    private async Task getDataKindBooks()
    {
        ListKindBooks = new List<KindBookModel>();
        SelectedKindBooks = new List<KindBookModel>();
        ListKindBooks = await _masterDataService!.GetDataKindBooksAsync();
    }

    #endregion

    #region Protected Functions

    protected async void ReLoadDataHandler()
    {
        try
        {
            IsInitialDataLoadComplete = false;
            await getDataKindBooks();
        }
        catch (Exception ex)
        {
            _logger!.LogError(ex, "UserController", "ReLoadDataHandler");
            ShowError(ex.Message);
        }
        finally
        {
            IsInitialDataLoadComplete = true;
            await InvokeAsync(StateHasChanged);
        }
    }

    protected void OnOpenDialogHandler(EnumType pAction = EnumType.Add, KindBookModel? pItemDetails = null)
    {
        try
        {
            if (pAction == EnumType.Add)
            {
                IsCreate = true;
                KindBookUpdate = new KindBookModel();
            }
            else
            {
                KindBookUpdate = new KindBookModel();
                KindBookUpdate.KindBookId = pItemDetails!.KindBookId;
                KindBookUpdate.KindBookName = pItemDetails.KindBookName;
                KindBookUpdate.Description = pItemDetails.Description;
                KindBookUpdate.DateCreate = pItemDetails.DateCreate;
                KindBookUpdate.UserCreate = pItemDetails.UserCreate;
                KindBookUpdate.LocationId = pItemDetails.LocationId;
                KindBookUpdate.LocationName = pItemDetails.LocationName;
                IsCreate = false;
            }
            IsShowDialog = true;
            _EditContext = new EditContext(KindBookUpdate);
        }
        catch (Exception ex)
        {
            _logger!.LogError(ex, "UserController", "OnOpenDialogHandler");
            ShowError(ex.Message);
        }
    }

    protected async void SaveDataHandler(EnumType pEnum = EnumType.SaveAndClose)
    {
        try
        {
            string sAction = IsCreate ? nameof(EnumType.Add) : nameof(EnumType.Update);
            var checkData = _EditContext!.Validate();
            if (!checkData) return;
            await ShowLoader();
            bool isSuccess = await _masterDataService!.UpdateKindBookAsync(JsonConvert.SerializeObject(KindBookUpdate), sAction, pUserId);
            if (isSuccess)
            {
                await getDataKindBooks();
                IsShowDialog = false;
                return;
            }
        }
        catch (Exception ex)
        {
            _logger!.LogError(ex, "UserController", "SaveDataHandler");
            ShowError(ex.Message);
        }
        finally
        {
            await Task.Delay(75);
            await ShowLoader(false);
            await InvokeAsync(StateHasChanged);
        }
    }
    protected void OnRowDoubleClickHandler(GridRowClickEventArgs args) => OnOpenDialogHandler(EnumType.Update, args.Item as KindBookModel);

    protected async void DeleteDataHandler()
    {
        try
        {
            if(SelectedKindBooks == null || !SelectedKindBooks.Any())
            {
                ShowWarning(DefaultConstants.MESSAGE_NO_CHOSE_DATA);
                return;
            }
            var confirm = await _rDialogs!.ConfirmAsync($" {DefaultConstants.MESSAGE_CONFIRM_DELETE} ");
            if (!confirm) return;
            await ShowLoader();
            bool isSuccess = await _masterDataService!.DeleteDataAsync(nameof(EnumTable.KindBooks), "", string.Join(",", SelectedKindBooks.Select(m=>m.KindBookId)), pUserId);
            if(isSuccess)
            {
                await getDataKindBooks();
            }
        }
        catch (Exception ex)
        {
            _logger!.LogError(ex, "UserController", "DeleteDataHandler");
            ShowError(ex.Message);
        }
        finally
        {
            await Task.Delay(75);
            await ShowLoader(false);
            await InvokeAsync(StateHasChanged);
        }
    }
    #endregion
}
