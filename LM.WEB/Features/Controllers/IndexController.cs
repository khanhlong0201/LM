using LM.Models;
using LM.WEB.Models;
using LM.WEB.Services;
using LM.WEB.Shared;
using Microsoft.AspNetCore.Components;

namespace LM.WEB.Features.Controllers
{
    public class IndexController : LMControllerBase
    {
        #region Dependency Injection

        [Inject] private ILogger<IndexController>? _logger { get; init; }
        [Inject] private ICliDocumentService? _documentService { get; init; }
        [Inject] private NavigationManager? _navigationManager { get; set; }

        #endregion Dependency Injection

        #region Properties
        public bool IsInitialDataLoadComplete { get; set; } = true;
        public List<BorrowOrderModel>? ListDocuments { get; set; }
        public IEnumerable<BorrowOrderModel>? SelectedDocuments { get; set; } = new List<BorrowOrderModel>();

        public int intTotal { get; set; }
        public int intBorrowing { get; set; }
        public int intClosed { get; set; }
        public int intDemurrage { get; set; }
        public int intToday { get; set; }
        public int intPending { get; set; }
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
                    //StartDate = _dateTimeService!.GetCurrentVietnamTime();
                    //StartTime = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, 23, 0, 0);
                    await _progressService!.SetPercent(0.4);
                    await showReport();
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

        #endregion Override Functions

        #region Private Functions

        private async Task showReport()
        {
            Dictionary<string, int>? keyValues = await _documentService!.GetReportIndexAsync();
            if (keyValues == null) return;
            if (keyValues.ContainsKey("intTotal")) intTotal = keyValues["intTotal"];
            if (keyValues.ContainsKey("intBorrowing")) intBorrowing = keyValues["intBorrowing"];
            if (keyValues.ContainsKey("intClosed")) intClosed = keyValues["intClosed"];
            if (keyValues.ContainsKey("intDemurrage")) intDemurrage = keyValues["intDemurrage"];
            if (keyValues.ContainsKey("intToday")) intToday = keyValues["intToday"];
            if (keyValues.ContainsKey("intPending")) intPending = keyValues["intPending"];
        }    
        #endregion
    }
}
