using LM.Models;
using LM.WEB.Models;
using LM.WEB.Services;
using LM.WEB.Shared;
using Microsoft.AspNetCore.Components;

namespace LM.WEB.Features.Controllers
{
    public class StaffController : LMControllerBase
    {
        #region Dependency Injection
        [Inject] private ILogger<StaffController>? _logger { get; init; }
        [Inject] private ICliMasterDataService? _masterDataService { get; init; }
        #endregion

        public bool IsInitialDataLoadComplete { get; set; } = true;
        public List<StaffModel>? ListStaffs { get; set; }

        #region Override Functions
        protected override async Task OnInitializedAsync()
        {
            try
            {
                await base.OnInitializedAsync();
                ListBreadcrumbs = new List<BreadcrumLModel>
                {
                    new BreadcrumLModel() { Text = "Trang chủ", IsShowIcon = true, Icon = "fa-solid fa-house-chimney" },
                    new BreadcrumLModel() { Text = "Hệ thống" },
                    new BreadcrumLModel() { Text = "Giáo viên, cán bộ, sinh viên" }
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
            ListStaffs = new List<StaffModel>();
            ListStaffs = await _masterDataService!.GetStaffsAsync();
        }
        #endregion

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
                await Task.Delay(500); 
                IsInitialDataLoadComplete = true;
                await InvokeAsync(StateHasChanged);
            }
        }
    }
}
