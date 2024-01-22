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
public class PublisherController : LMControllerBase
{
    #region Dependency Injection
    [Inject] private ILogger<PublisherController>? _logger { get; init; }
    [Inject] private ICliMasterDataService? _masterDataService { get; init; }
    #endregion

    #region Properties
    public bool IsInitialDataLoadComplete { get; set; } = true;
    public List<PublisherModel>? ListPublishers { get; set; }
    public IEnumerable<PublisherModel>? SelectedPublishers { get; set; } = new List<PublisherModel>();
    public PublisherModel PublisherUpdate { get; set; } = new PublisherModel();
    public EditContext? _EditContext { get; set; }
    public bool IsShowDialog { get; set; }
    public bool IsCreate { get; set; } = true;
    public HConfirm? _rDialogs { get; set; }
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
                    new BreadcrumLModel() { Text = "Nhà xuất bản" }
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
                await getDataPublishers();
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
    private async Task getDataPublishers()
    {
        ListPublishers = new List<PublisherModel>();
        SelectedPublishers = new List<PublisherModel>();
        ListPublishers = await _masterDataService!.GetDataPublishersAsync();
    }

    #endregion

    #region Protected Functions

    protected async void ReLoadDataHandler()
    {
        try
        {
            IsInitialDataLoadComplete = false;
            await getDataPublishers();
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

    protected void OnOpenDialogHandler(EnumType pAction = EnumType.Add, PublisherModel? pItemDetails = null)
    {
        try
        {
            if (pAction == EnumType.Add)
            {
                IsCreate = true;
                PublisherUpdate = new PublisherModel();
            }
            else
            {
                PublisherUpdate.PublisherId = pItemDetails!.PublisherId;
                PublisherUpdate.PublisherName = pItemDetails.PublisherName;
                PublisherUpdate.Description = pItemDetails.Description;
                PublisherUpdate.DateCreate = pItemDetails.DateCreate;
                PublisherUpdate.UserCreate = pItemDetails.UserCreate;
                IsCreate = false;
            }
            IsShowDialog = true;
            _EditContext = new EditContext(PublisherUpdate);
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
            bool isSuccess = await _masterDataService!.UpdatePublisherAsync(JsonConvert.SerializeObject(PublisherUpdate), sAction, pUserId);
            if (isSuccess)
            {
                await getDataPublishers();
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
            await ShowLoader(false);
            await InvokeAsync(StateHasChanged);
        }
    }
    protected void OnRowDoubleClickHandler(GridRowClickEventArgs args) => OnOpenDialogHandler(EnumType.Update, args.Item as PublisherModel);

    protected async void DeleteDataHandler()
    {
        try
        {
            if(SelectedPublishers == null || !SelectedPublishers.Any())
            {
                ShowWarning(DefaultConstants.MESSAGE_NO_CHOSE_DATA);
                return;
            }
            var confirm = await _rDialogs!.ConfirmAsync($" {DefaultConstants.MESSAGE_CONFIRM_DELETE} ");
            if (!confirm) return;
            await ShowLoader();
            bool isSuccess = await _masterDataService!.DeleteDataAsync(nameof(EnumTable.Publishers), "", string.Join(",", SelectedPublishers.Select(m=>m.PublisherId)), pUserId);
            if(isSuccess)
            {
                await getDataPublishers();
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
