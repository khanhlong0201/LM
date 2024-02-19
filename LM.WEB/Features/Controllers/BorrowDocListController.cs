using LM.Models;
using LM.Models.Shared;
using LM.WEB.Models;
using LM.WEB.Services;
using LM.WEB.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using NPOI.POIFS.FileSystem;
using Telerik.Blazor;
using Telerik.Blazor.Components;

namespace LM.WEB.Features.Controllers
{
    public class BorrowDocListController : LMControllerBase
    {
        #region Dependency Injection
        [Inject] private ILogger<BorrowDocListController>? _logger { get; init; }
        [Inject] private ICliDocumentService? _documentService { get; init; }
        [Inject] private NavigationManager? _navManager { get; init; }
        #endregion

        #region Properties
        public bool IsInitialDataLoadComplete { get; set; } = true;
        public List<BorrowOrderModel>? ListDocuments { get; set; }
        public IEnumerable<BorrowOrderModel>? SelectedDocuments { get; set; } = new List<BorrowOrderModel>();
        public List<ComboboxModel>? ListStatus { get; set; }
        public List<ComboboxModel>? ListTypeBO { get; set; }
        public SearchModel ItemFilter = new SearchModel();
        public string? ReasonDeny { get; set; } // lý do hủy
        public bool IsShowDialogDelete { get; set; }

        [CascadingParameter]
        public DialogFactory? _rDialogs { get; set; }
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
                    new BreadcrumLModel() { Text = "Quản lý mượn trả" },
                    new BreadcrumLModel() { Text = "Theo dõi" },
                };
                await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                ListStatus = new List<ComboboxModel>()
                {
                    new ComboboxModel() {Code = nameof(DocStatus.Pending), Name = "Chờ xử lý"},
                    new ComboboxModel() {Code = nameof(DocStatus.Borrowing), Name = "Đang mượn"},
                    new ComboboxModel() {Code = nameof(DocStatus.Closed), Name = "Hoàn thành"},
                    new ComboboxModel() {Code = nameof(DocStatus.Cancled), Name = "Đã hủy phiếu"},
                    new ComboboxModel() {Code = nameof(DocStatus.All), Name = "Tất cả"},
                };
                ListTypeBO = new List<ComboboxModel>()
                {
                    new ComboboxModel() {Code = nameof(DocStatus.All), Name = "Tất cả"},
                    new ComboboxModel() {Code = "Online", Name = "Online"},
                    new ComboboxModel() {Code = "Offline", Name = "Offline"},
                };
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
                    ItemFilter.StatusId = nameof(DocStatus.All);
                    ItemFilter.TypeBO = nameof(DocStatus.All);
                    ItemFilter.FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    ItemFilter.ToDate = _dateTimeService!.GetCurrentVietnamTime();
                    // đọc giá tri câu query
                    var uri = _navManager?.ToAbsoluteUri(_navManager.Uri);
                    if (uri != null && QueryHelpers.ParseQuery(uri.Query).Count > 0)
                    {
                        string key = uri.Query.Substring(5); // để tránh parse lỗi;    
                        Dictionary<string, string> pParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(EncryptHelper.Decrypt(key));
                        if (pParams != null && pParams.Any() && pParams.ContainsKey("pStatusId")) ItemFilter.StatusId = pParams["pStatusId"];
                    }
                    //
                    await _progressService!.SetPercent(0.4);
                    await getDataDocuments();
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
        private async Task getDataDocuments()
        {
            ListDocuments = new List<BorrowOrderModel>();
            SelectedDocuments = new List<BorrowOrderModel>();
            ItemFilter.UserId = pUserId;
            ItemFilter.IsAdmin = pIsAdmin;
            ItemFilter.Type = "ALL";
            ListDocuments = await _documentService!.GetBorrowOrdersAsync(ItemFilter);
        }

        #endregion

        #region Protected Functions
        protected async void ReLoadDataHandler()
        {
            try
            {
                if (ItemFilter.FromDate.HasValue && ItemFilter.ToDate.HasValue
                    && ItemFilter.FromDate.Value.Date > ItemFilter.ToDate.Value.Date)
                {
                    ShowWarning("Dữ liệu tìm kiếm không hợp lệ. [Từ ngày] <= [Đến ngày]");
                    return;
                }
                await ShowLoader();
                await getDataDocuments();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "BorrowDocListController", "ReLoadDataHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await ShowLoader(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// double click vào xem chi tiết
        /// </summary>
        /// <param name="args"></param>
        protected async void OnRowDoubleClickHandler(GridRowClickEventArgs args)
        {
            try
            {
                BorrowOrderModel? oItem = (args.Item as BorrowOrderModel);
                if (oItem == null) return;
                Dictionary<string, string> pParams = new Dictionary<string, string>
                {
                    { "pVoucherNo", $"{oItem.VoucherNo}"},
                    { "pIsCreate", $"{false}" },
                };
                string key = EncryptHelper.Encrypt(JsonConvert.SerializeObject(pParams)); // mã hóa key
                _navManager!.NavigateTo($"/document-create?key={key}");
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "BorrowDocListController", "OnRowDoubleClickHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await InvokeAsync(StateHasChanged);
            }
        }


        protected void OpenDialogDeleteHandler()
        {
            try
            {
                if (SelectedDocuments == null || !SelectedDocuments.Any())
                {
                    ShowWarning("Vui lòng chọn dòng để hủy!");
                    return;
                }
                var checkData = SelectedDocuments.FirstOrDefault(m => m.StatusCode != nameof(DocStatus.Pending));
                if (checkData != null)
                {
                    ShowWarning("Chỉ được phép hủy các phiếu mượn có tình trạng [Chờ xử lý]!");
                    return;
                }
                ReasonDeny = "";
                IsShowDialogDelete = true;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "SalesDocListController", "OpenDialogDeleteHandler");
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// xác nhận hủy phiếu mượn
        /// </summary>
        protected async void CancleDocListHandler()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ReasonDeny))
                {
                    ShowWarning("Vui lòng điền lý do hủy đơn!");
                    return;
                }
                await ShowLoader();
                bool isSuccess = await _documentService!.CancleDocList(string.Join(",", SelectedDocuments!.Select(m => m.VoucherNo))
                        , ReasonDeny, pUserId, nameof(EnumTable.BorrowOrders));
                if (isSuccess)
                {
                    IsShowDialogDelete = false;
                    await getDataDocuments();
                }
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "SalesDocListController", "CancleDocListHandler");
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
