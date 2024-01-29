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
public class SeriesController : LMControllerBase
{
    #region Dependency Injection
    [Inject] private ILogger<BatchController>? _logger { get; init; }
    [Inject] private ICliMasterDataService? _masterDataService { get; init; }
    [Inject] private IConfiguration? _configuration { get; set; }
    #endregion

    #region Properties
    public bool IsInitialDataLoadComplete { get; set; } = true;
    public List<SeriesModel>? ListSeries { get; set; } = new List<SeriesModel>();
    public IEnumerable<SeriesModel>? SelectedSeries { get; set; } = new List<SeriesModel>();
    public SeriesModel SeriesUpdate { get; set; } = new SeriesModel();
    public EditContext? _EditContext { get; set; }
    public bool IsShowDialog { get; set; }
    public bool IsCreate { get; set; } = true;
    public HConfirm? _rDialogs { get; set; }
    public List<BookModel>? ListBooks { get; set; } = new List<BookModel>();
    public List<KindBookModel>? ListKindBooks { get; set; } = new List<KindBookModel>();
    public List<PublisherModel>? ListPublishers { get; set; } = new List<PublisherModel>();
    public List<BatchModel>? ListBatchs { get; set; } = new List<BatchModel>();
    public List<SeriesModel>? ListSeriesCreate { get; set; } = new List<SeriesModel>();
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
                    new BreadcrumLModel() { Text = "Seri Sách" }
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
                await getDataSeries();
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
    private async Task getDataSeries()
    {
        ListSeries = new List<SeriesModel>();
        SelectedSeries = new List<SeriesModel>();
        ListSeries = await _masterDataService!.GetDataSeriesAsync(-1);
    }

    private async Task getDataCombo()
    {
        ListBooks = new List<BookModel>();
        ListBooks = await _masterDataService!.GetDataBooksAsync(ItemFilter);
        ListBatchs = await _masterDataService!.GetDataBatchsAsync(-1);
        ListKindBooks = await _masterDataService!.GetDataKindBooksAsync();
        ListPublishers = await _masterDataService!.GetDataPublishersAsync();
    }

    #endregion

    #region Protected Functions
    // Phương thức callback để xử lý sự thay đổi của ListSelectReturn
    //protected async Task HandleListSelectReturnChanged(List<SeriesModel> newList)
    //{
    //    ListSeriesCreate = newList;
    //    StateHasChanged(); // Cập nhật trạng thái UI nếu cần thiết
    //}
    protected async void ReLoadDataHandler()
    {
        try
        {
            IsInitialDataLoadComplete = false;
            await getDataSeries();
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

    protected async Task OnOpenDialogHandler(EnumType pAction = EnumType.Add, SeriesModel? pItemDetails = null)
    {
        try
        {
            if (pAction == EnumType.Add)
            {
                IsCreate = true;
                SeriesUpdate = new SeriesModel();
            }
            else
            {
                SeriesUpdate.BatchId = pItemDetails!.BatchId;
                SeriesUpdate.BookId = pItemDetails!.BookId;
                SeriesUpdate.BookName = pItemDetails!.BookName;
                SeriesUpdate.DateCreate = pItemDetails.DateCreate;
                SeriesUpdate.UserCreate = pItemDetails.UserCreate;
                IsCreate = false;
            }
            IsShowDialog = true;
            _EditContext = new EditContext(SeriesUpdate);
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
            if(ListSeriesCreate != null && ListSeriesCreate.Count <= 0)
            {
                _toastService.ShowWarning("Không có số seri nào để lưu !");
                return;
            }
            var checkNullSeriCodeFirst = ListSeriesCreate.Where(d => d.SeriesCode + "" == "").FirstOrDefault();
            if (checkNullSeriCodeFirst !=null)
            {
                _toastService.ShowWarning($"Bạn chưa nhập số seri ở STT {checkNullSeriCodeFirst.STT} !");
                return;
            }
            await ShowLoader();
            await _masterDataService!.UpdateSeriesAsync(JsonConvert.SerializeObject(SeriesUpdate), JsonConvert.SerializeObject(ListSeriesCreate), sAction, pUserId);
            await getDataSeries();
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
    protected void OnRowDoubleClickHandler(GridRowClickEventArgs args) => OnOpenDialogHandler(EnumType.Update, args.Item as SeriesModel);

    protected async void DeleteDataHandler()
    {
        try
        {
            if (SelectedSeries == null || !SelectedSeries.Any())
            {
                ShowWarning(DefaultConstants.MESSAGE_NO_CHOSE_DATA);
                return;
            }
            var confirm = await _rDialogs!.ConfirmAsync($" {DefaultConstants.MESSAGE_CONFIRM_DELETE} ");
            if (!confirm) return;
            await ShowLoader();
            bool isSuccess = await _masterDataService!.DeleteDataAsync(nameof(EnumTable.Batchs), "", string.Join(",", SelectedSeries.Select(m => m.BatchId)), pUserId);
            if (isSuccess)
            {
                await getDataSeries();
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
