using LM.Models;
using LM.WEB.Models;
using LM.WEB.Services;
using LM.WEB.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using System.Net;
using Telerik.Blazor;
using Telerik.Blazor.Components;

namespace LM.WEB.Features.Controllers
{
    public class DocumentController : LMControllerBase
    {
        #region Dependency Injection
        [Inject] private ILogger<DocumentController>? _logger { get; init; }
        [Inject] private ICliMasterDataService? _masterDataService { get; init; }
        [Inject] private ICliDocumentService? _documentService { get; init; }
        #endregion

        #region Properties
        public BorrowOrderModel DocumentUpdate { get; set; } = new BorrowOrderModel();
        public List<BODetailModel>? ListBODetails { get; set; }
        public IEnumerable<BODetailModel> SelectedBODetails { get; set; } = new List<BODetailModel>();
        public TelerikGrid<BODetailModel>? RefListBODetails { get; set; }
        public bool IsShowDialog { get; set; }

        public List<StaffModel>? ListStaffs { get; set; }
        public IEnumerable<StaffModel> SelectedStaffs { get; set; } = new List<StaffModel>();
        public bool IsShowDialogStaff { get; set; }

        public bool IsInitialDataLoadComplete { get; set; } = true;
        public List<BookSerialModel>? ListBookSerials { get; set; }
        public IEnumerable<BookSerialModel> SelectedBookSerials { get; set; } = new List<BookSerialModel>();
        public const string DATA_EMPTY = "Chưa cập nhật";
        public const string TYPE_BO = "Offline";
        public bool pIsLockPage { get; set; } = false;
        public int pDocEntry { get; set; } = 0;

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
                    new BreadcrumLModel() { Text = "Tạo phiếu mượn" }
                };
                await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);

                DocumentUpdate.VoucherNo = DATA_EMPTY;
                DocumentUpdate.StatusName = DATA_EMPTY;
                DocumentUpdate.DateCreate = DateTime.Now;
                DocumentUpdate.DocDate = DateTime.Now;
                DocumentUpdate.TypeBO = TYPE_BO;
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

        private async Task getStaffs()
        {
            ListStaffs = new List<StaffModel>();
            SelectedStaffs = new List<StaffModel>();
            ListStaffs = await _masterDataService!.GetStaffsAsync();
        }

        private bool validateData(bool isCreate)
        {
            if (string.IsNullOrWhiteSpace(DocumentUpdate.StaffCode))
            {
                ShowWarning("Vui lòng điền thông tin mã thẻ/mã số sinh viên!");
                return false;
            }
            if (string.IsNullOrWhiteSpace(DocumentUpdate.FullName))
            {
                ShowWarning("Vui lòng điền thông tin Tên cán bộ, giáo viên, sinh viên!");
                return false;
            }
            if (DocumentUpdate.PromiseDate is null)
            {
                ShowWarning("Vui lòng điền ngày hẹn trả sách!");
                return false;
            }
            if (isCreate && DocumentUpdate.PromiseDate.Value.Date < DateTime.Now.Date)
            {
                ShowWarning("Ngày hẹn trả >= ngày hiện tại!");
                return false;
            }
            return true;
        }    
        #endregion

        #region Protected Functions
        protected async void ReLoadDataHandler(bool isBook = true)
        {
            try
            {
                IsInitialDataLoadComplete = false;
                if (isBook) await getBookSerials();
                else await getStaffs();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "DocumentController", "ReLoadDataHandler");
                ShowError(ex.Message);
            }
            finally
            {
                IsInitialDataLoadComplete = true;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async void OnOpenDialogHandler(bool isBook = true)
        {
            try
            {
                await ShowLoader();
                if(isBook)
                {
                    // tìm kiếm sách
                    await getBookSerials();
                    IsShowDialog = true;
                }  
                else
                {
                    // tìm kiếm nhân viên
                    await getStaffs();
                    IsShowDialogStaff = true;

                }    
                

            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "DocumentController", "OnOpenDialogHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await Task.Delay(75);
                await ShowLoader(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// xóa dữ liệu trên lưới
        /// </summary>
        protected void RemoveBooksHandler()
        {
            try
            {
                if(ListBODetails == null || !ListBODetails.Any())
                {
                    ShowWarning("không có dữ liệu sách mượn");
                    return;
                }    
                if (SelectedBODetails == null || !SelectedBODetails.Any())
                {
                    ShowWarning("Vui lòng chọn dòng để xóa");
                    return;
                }
                RefListBODetails?.Rebind();
                StateHasChanged();
            }
            catch(Exception ex)
            {
                _logger!.LogError(ex, "DocumentController", "RemoveBooksHandler");
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// xóa dữ liệu trên lưới
        /// </summary>
        protected void AddBooksHandler()
        {
            try
            {
                if (ListBookSerials == null || !ListBookSerials.Any())
                {
                    ShowWarning("không có dữ liệu sách!");
                    return;
                }
                if (SelectedBookSerials == null || !SelectedBookSerials.Any())
                {
                    ShowWarning("Vui lòng chọn sách cần mượn!");
                    return;
                }
                if (ListBODetails == null) ListBODetails = new List<BODetailModel>();

                var lstData = SelectedBookSerials.Select(m => new BODetailModel()
                {
                    BookSerialId = m.Id,
                    BookId = m.BookID,
                    BookName = m.BookName,
                    NoteForAll = m.NoteForAll,
                    SerialNumber = m.SerialNumber,
                    Quantity = 1
                });
                ListBODetails.AddRange(lstData);
                RefListBODetails?.Rebind();
                IsShowDialog = false;
                StateHasChanged();
                
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "DocumentController", "RemoveBooksHandler");
                ShowError(ex.Message);
            }
        }

        protected void ChoseStaffHandler()
        {
            try
            {
                if (ListStaffs == null || !ListStaffs.Any())
                {
                    ShowWarning("không có dữ liệu người mượn!");
                    return;
                }
                if (SelectedStaffs == null || !SelectedStaffs.Any())
                {
                    ShowWarning("Vui lòng chọn người cần mượn!");
                    return;
                }
                var oItem = SelectedStaffs.First();
                DocumentUpdate.StaffCode = oItem.StaffCode;
                DocumentUpdate.FullName = oItem.FullName;
                DocumentUpdate.Department = oItem.Department;
                DocumentUpdate.PhoneNumber = oItem.PhoneNumber;
                DocumentUpdate.StaffTypeName = oItem.StaffTypeName;
                DocumentUpdate.Email = oItem.Email;
                IsShowDialogStaff = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "DocumentController", "ChoseStaffHandler");
                ShowError(ex.Message);
            }
        }    

        protected async void SaveDocHandler(EnumType pProcess = EnumType.Update)
        {
            try
            {
                bool isCreate = true;
                string strAction = nameof(EnumType.Add);
                if (pDocEntry > 0)
                {
                    isCreate = false;
                    strAction = nameof(EnumType.Update);
                }    
                if (!validateData(isCreate)) return;

                DocumentUpdate.StatusCode = nameof(DocStatus.Pending);
                bool isSuccess = await _documentService!.UpdateBorrowOrder(JsonConvert.SerializeObject(DocumentUpdate)
                    , JsonConvert.SerializeObject(ListBODetails), strAction, pUserId);

                if (isSuccess)
                {
                    if(isCreate)
                    {
                        return;
                    }   
                    else
                    {

                    }    
                }    
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "DocumentController", "SaveDocHandler");
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
}
