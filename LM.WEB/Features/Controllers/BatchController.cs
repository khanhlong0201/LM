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
public class BatchController : LMControllerBase
{
    #region Dependency Injection
    [Inject] private ILogger<BatchController>? _logger { get; init; }
    [Inject] private ICliMasterDataService? _masterDataService { get; init; }
    [Inject] private IConfiguration? _configuration { get; set; }
    #endregion

    #region Properties
    public bool IsInitialDataLoadComplete { get; set; } = true;
    public List<BatchModel>? ListBatchs { get; set; }
    public IEnumerable<BatchModel>? SelectedBatchs { get; set; } = new List<BatchModel>();
    public BatchModel BatchUpdate { get; set; } = new BatchModel();
    public EditContext? _EditContext { get; set; }
    public bool IsShowDialog { get; set; }
    public bool IsCreate { get; set; } = true;
    public HConfirm? _rDialogs { get; set; }
    public List<BookModel>? ListBooks { get; set; }
    SearchModel ItemFilter { get; set; } = new SearchModel();
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
                    new BreadcrumLModel() { Text = "Lô Sách" }
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
                await getDataCombo();
                await getDataBatchs();
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
    private async Task getDataBatchs()
    {
        ListBatchs = new List<BatchModel>();
        SelectedBatchs = new List<BatchModel>();
        ListBatchs = await _masterDataService!.GetDataBatchsAsync(-1);
    }

    private async Task getDataCombo()
    {
        ListBooks = new List<BookModel>();
        ListBooks = await _masterDataService!.GetDataBooksAsync(ItemFilter);
    }

    #endregion

    #region Protected Functions
    protected async void ReLoadDataHandler()
    {
        try
        {
            IsInitialDataLoadComplete = false;
            await getDataBatchs();
        }
        catch (Exception ex)
        {
            _logger!.LogError(ex, "BatchController", "ReLoadDataHandler");
            ShowError(ex.Message);
        }
        finally
        {
            IsInitialDataLoadComplete = true;
            await InvokeAsync(StateHasChanged);
        }
    }

    protected async Task OnOpenDialogHandler(EnumType pAction = EnumType.Add, BatchModel? pItemDetails = null)
    {
        try
        {
            if (pAction == EnumType.Add)
            {
                IsCreate = true;
                BatchUpdate = new BatchModel();
            }
            else
            {
                BatchUpdate.BatchId = pItemDetails!.BatchId;
                BatchUpdate.BookId = pItemDetails!.BookId;
                BatchUpdate.BookName = pItemDetails!.BookName;
                BatchUpdate.Price = pItemDetails!.Price;
                BatchUpdate.Qty = pItemDetails!.Qty;
                BatchUpdate.DateCreate = pItemDetails.DateCreate;
                BatchUpdate.UserCreate = pItemDetails.UserCreate;
                IsCreate = false;
            }
            IsShowDialog = true;
            _EditContext = new EditContext(BatchUpdate);
        }
        catch (Exception ex)
        {
            _logger!.LogError(ex, "BookController", "OnOpenDialogHandler");
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

            if (BatchUpdate != null && BatchUpdate.BookId ==0)
            {
                _toastService.ShowWarning("Bạn phải chọn sách !");
                return;
            }
            if (BatchUpdate.Qty != null && BatchUpdate.Qty <= 0)
            {
                _toastService.ShowWarning("Số lượng nhập lô phải > 0 !");
                return;
            }
            if (BatchUpdate.Price != null &&  BatchUpdate.Price <= 0)
            {
                _toastService.ShowWarning("Giá phải > 0 !");
                return;
            }
            await ShowLoader();
            await _masterDataService!.UpdateBatchAsync(JsonConvert.SerializeObject(BatchUpdate), sAction, pUserId);
            await getDataBatchs();
            IsShowDialog = false;
        }
        catch (Exception ex)
        {
            _logger!.LogError(ex, "BookController", "SaveDataHandler");
            ShowError(ex.Message);
        }
        finally
        {
            await ShowLoader(false);
            await InvokeAsync(StateHasChanged);
        }
    }
    protected void OnRowDoubleClickHandler(GridRowClickEventArgs args) => OnOpenDialogHandler(EnumType.Update, args.Item as BatchModel);

    protected async void DeleteDataHandler()
    {
        try
        {
            if (SelectedBatchs == null || !SelectedBatchs.Any())
            {
                ShowWarning(DefaultConstants.MESSAGE_NO_CHOSE_DATA);
                return;
            }
            var confirm = await _rDialogs!.ConfirmAsync($" {DefaultConstants.MESSAGE_CONFIRM_DELETE} ");
            if (!confirm) return;
            await ShowLoader();
            bool isSuccess = await _masterDataService!.DeleteDataAsync(nameof(EnumTable.Batchs), "", string.Join(",", SelectedBatchs.Select(m => m.BatchId)), pUserId);
            if (isSuccess)
            {
                await getDataBatchs();
            }
        }
        catch (Exception ex)
        {
            _logger!.LogError(ex, "UserController", "DeleteDataHandler");
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
