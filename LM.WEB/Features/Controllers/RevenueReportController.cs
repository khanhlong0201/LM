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
    public class RevenueReportController : LMControllerBase
    {
        #region Dependency Injection
        [Inject] private ILogger<RevenueReportController>? _logger { get; init; }
        [Inject] private ICliDocumentService? _documentervice { get; init; }
        #endregion
        #region Properties
        public string[]? ResportTitle { get; set; }
        public List<int>? ListYears { get; set; }
        public int pYearDefault { get; set; }
        public List<ReportModel>? DataReport { get; set; }
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
                    new BreadcrumLModel() { Text = "Báo cáo" },
                    new BreadcrumLModel() { Text = "Báo cáo tổng thể số lượng sách mượn của thư viện" }
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

                    ListYears = new List<int>();
                    pYearDefault = DateTime.Now.Year;
                    for (int i = 2023; i < (DateTime.Now.Year + 1); i++)
                    {
                        ListYears.Add(i);
                    }
                    await _progressService!.SetPercent(0.4);
                    await getDataReport();
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
        private async Task getDataReport()
        {
            DataReport = new List<ReportModel>();
            DataReport = await _documentervice!.GetRevenueReportAsync(pYearDefault);
            ResportTitle = DataReport?.Select(m => m.Title).ToArray();
        }
        #endregion

        #region Protected Functions
        protected async void ReLoadDataHandler()
        {
            try
            {
                await ShowLoader();
                await getDataReport();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "RevenueReportController", "ReLoadDataHandler");
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
