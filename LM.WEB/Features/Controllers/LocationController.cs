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
    public class LocationController : LMControllerBase
    {
        #region Dependency Injection
        [Inject] private ILogger<LocationController>? _logger { get; init; }
        [Inject] private ICliMasterDataService? _masterDataService { get; init; }
        [Inject] private IConfiguration? _configuration { get; set; }
        #endregion

        public bool IsInitialDataLoadComplete { get; set; } = true;
        public List<LocationModel>? ListLocations { get; set; }
        public IEnumerable<LocationModel> SelectedLocations { get; set; } = new List<LocationModel>();
        public LocationModel LocationUpdate { get; set; } = new LocationModel();

        public EditContext? _EditContext { get; set; }
        public bool IsShowDialog { get; set; }
        public bool IsCreate { get; set; } = true;
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
                    new BreadcrumLModel() { Text = "Danh mục" },
                    new BreadcrumLModel() { Text = "Vị trí sách" }
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
                    await getLocations();
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
        private async Task getLocations()
        {
            ListLocations = new List<LocationModel>();
            SelectedLocations = new List<LocationModel>();
            ListLocations = await _masterDataService!.GetLocationsAsync();
        }
        #endregion

        #region Protected Functions

        protected async void ReLoadDataHandler()
        {
            try
            {
                IsInitialDataLoadComplete = false;
                await getLocations();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "LocationController", "ReLoadDataHandler");
                ShowError(ex.Message);
            }
            finally
            {
                IsInitialDataLoadComplete = true;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected void OnOpenDialogHandler(EnumType pAction = EnumType.Add, LocationModel? pItemDetails = null)
        {
            try
            {
                if (pAction == EnumType.Add)
                {
                    IsCreate = true;
                    LocationUpdate = new LocationModel();
                }    
                else
                {
                    LocationUpdate.Id = pItemDetails!.Id;
                    LocationUpdate.LocationName = pItemDetails!.LocationName;
                    LocationUpdate.Description = pItemDetails!.Description;
                    IsCreate = false;
                }

                _EditContext = new EditContext(LocationUpdate);
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
                bool isSuccess = await _masterDataService!.UpdateLocationAsync(JsonConvert.SerializeObject(LocationUpdate), sAction, pUserId);
                if (isSuccess)
                {
                    await getLocations();
                    IsShowDialog = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "LocationController", "SaveDataHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await Task.Delay(75);
                await ShowLoader(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        protected void OnRowDoubleClickHandler(GridRowClickEventArgs args) => OnOpenDialogHandler(EnumType.Update, args.Item as LocationModel);

        protected async void DeleteDataHandler()
        {
            try
            {
                if (SelectedLocations == null || !SelectedLocations.Any())
                {
                    ShowWarning(DefaultConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                var confirm = await _rDialogs!.ConfirmAsync($" {DefaultConstants.MESSAGE_CONFIRM_DELETE} ");
                if (!confirm) return;
                await ShowLoader();
                bool isSuccess = await _masterDataService!.DeleteDataAsync(nameof(EnumTable.Locations), "", string.Join(",", SelectedLocations.Select(m => m.Id)), pUserId);
                if (isSuccess)
                {
                    await getLocations();
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
}
