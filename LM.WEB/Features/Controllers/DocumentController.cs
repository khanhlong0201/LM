using LM.Models;
using LM.Models.Shared;
using LM.WEB.Models;
using LM.WEB.Services;
using LM.WEB.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using NPOI.POIFS.FileSystem;
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
        [Inject] private NavigationManager? _navigationManager { get; init; }
        #endregion

        #region Properties
        public BorrowOrderModel DocumentUpdate { get; set; } = new BorrowOrderModel();
        public List<BODetailModel>? ListBODetails { get; set; }
        public IEnumerable<BODetailModel> SelectedBODetails { get; set; } = new List<BODetailModel>();
        public TelerikGrid<BODetailModel>? RefListBODetails { get; set; }
        public bool IsShowDialog { get; set; }
        public bool IsChoseSerial { get; set; } // nếu được chọn ở Nút serial

        public List<StaffModel>? ListStaffs { get; set; }
        public IEnumerable<StaffModel> SelectedStaffs { get; set; } = new List<StaffModel>();
        public bool IsShowDialogStaff { get; set; }

        public bool IsInitialDataLoadComplete { get; set; } = true;
        public List<BookSerialModel>? ListBookSerials { get; set; }
        public IEnumerable<BookSerialModel> SelectedBookSerials { get; set; } = new List<BookSerialModel>();
        public List<BookModel>? ListBooks { get; set; }
        public IEnumerable<BookModel>? SelectedBooks { get; set; } = new List<BookModel>();

        public const string DATA_EMPTY = "Chưa cập nhật";
        public bool pIsLockPage { get; set; } = false;
        public string pVoucherNo { get; set; } = string.Empty;
        public string pTypeBO { get; set; } = "Offline";
        public bool pIsCreate { get; set; } = true;

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
                DocumentUpdate.TypeBO = pTypeBO;
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
                    // đọc giá tri câu query
                    var uri = _navigationManager?.ToAbsoluteUri(_navigationManager.Uri);
                    if (uri != null && QueryHelpers.ParseQuery(uri.Query).Count > 0)
                    {
                        string key = uri.Query.Substring(5); // để tránh parse lỗi;    
                        Dictionary<string, string> pParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(EncryptHelper.Decrypt(key));
                        if (pParams != null && pParams.Any())
                        {
                            if (pParams.ContainsKey("pIsCreate")) pIsCreate = Convert.ToBoolean(pParams["pIsCreate"]);
                            if (pParams.ContainsKey("pVoucherNo")) pVoucherNo = Convert.ToString(pParams["pVoucherNo"]);
                        }
                    }
                    if (!pIsCreate)
                    {
                        // Vô từ page theo dõi
                        await showVoucher();
                    }    
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

        private async Task showVoucher()
        {
            if (string.IsNullOrEmpty(pVoucherNo))
            {
                ShowWarning("Vui lòng tải lại trang hoặc liên hệ IT để được hổ trợ");
                return;
            }

            Dictionary<string, string>? keyValues = await _documentService!.GetDocByIdAsync(pVoucherNo);
            if (keyValues == null) return;
            if (keyValues.ContainsKey("oHeader"))
            {
                DocumentUpdate = JsonConvert.DeserializeObject<BorrowOrderModel>(keyValues["oHeader"]);
                pIsLockPage = DocumentUpdate.StatusCode != nameof(DocStatus.Pending); // lock page
                //pIsLockPage = DocumentUpdate.StatusId != nameof(DocStatus.Pending); // lock page
            }
            if (keyValues.ContainsKey("oLine"))
            {
                ListBODetails = JsonConvert.DeserializeObject<List<BODetailModel>>(keyValues["oLine"]);
            }    
        }    

        private async Task getBooks()
        {
            ListBooks = new List<BookModel>();
            SelectedBooks = new List<BookModel>();
            SearchModel ItemFilter = new SearchModel();
            ListBooks = await _masterDataService!.GetDataBooksAsync(ItemFilter);
        }
        private async Task getBookSerials(int pBookId, string pBookName)
        {
            ListBookSerials = new List<BookSerialModel>();
            SelectedBookSerials = new List<BookSerialModel>();
            var lstData = await _masterDataService!.GetBookSerialsAsync();
            if(lstData != null && lstData.Any())
            {
                ListBookSerials = lstData.Where(m=>m.BookID == pBookId).ToList();
                return;
            }
            ShowInfo($"Không tìm thấy danh sách Serial của sách {pBookName}");

        }

        private async Task getStaffs()
        {
            ListStaffs = new List<StaffModel>();
            SelectedStaffs = new List<StaffModel>();
            LoginRequestModel loginRequestModel = new LoginRequestModel();
            loginRequestModel.IsLogin = false;
            ListStaffs = await _masterDataService!.GetStaffsAsync(loginRequestModel);
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

            if(ListBODetails == null || !ListBODetails.Any())
            {
                ShowWarning("Vui lòng chọn sách cần mượn!");
                return false;
            }    
            
            if(!isCreate && DocumentUpdate.TypeBO == "Online")
            {
                // đối với Qty trình mượn sách OnLine
                // Bắt buộc phải chọn lại Serial nếu chưa chọn
                var oCheckSerial = ListBODetails.FirstOrDefault(m => m.SerialNumber == "KHONGXACDINH");
                if(oCheckSerial != null)
                {
                    ShowWarning($"Vui lòng điền lại số Serial cho sách [{oCheckSerial.BookName}]!");
                    return false;
                }    
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
                if (isBook) await getBooks();
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

        /// <summary>
        /// Mở popup
        /// </summary>
        /// <param name="isBook"> Nếu true: mở sách, ngược lại mở Sinh viên</param>
        /// <param name="isSerial">Tìm kiếm theo serial</param>
        protected async void OnOpenDialogHandler(bool isBook = true, int bookId = -1)
        {
            try
            {
                await ShowLoader();
                IsChoseSerial = false;
                if (isBook)
                {
                    // tìm kiếm sách
                    ListBookSerials = new List<BookSerialModel>();
                    SelectedBookSerials = new List<BookSerialModel>();
                    await getBooks();
                    if(bookId > 0)
                    {
                        IsChoseSerial = true;
                        SelectedBooks = ListBooks?.Where(m => m.BookId == bookId);
                        await getBookSerials(bookId, "");
                    }    
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
        protected void RemoveBooksHandler(BODetailModel? oItem)
        {
            try
            {
                if (oItem == null) return;
                if(ListBODetails == null || !ListBODetails.Any())
                {
                    ShowWarning("không có dữ liệu sách mượn");
                    return;
                }    
                ListBODetails.Remove(oItem);
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

                // kiểm tra trùng
                var checkDub = ListBODetails.FirstOrDefault(m=> SelectedBookSerials.Select(m => m.Id).Contains(m.BookSerialId));
                if(checkDub != null)
                {
                    ShowWarning($"Trùng số Serial [{checkDub.SerialNumber}] của sách [{checkDub.BookName}]");
                    return;
                }    

                   
                var lstData = SelectedBookSerials.Select(m => new BODetailModel()
                {
                    BookSerialId = m.Id,
                    BookId = m.BookID,
                    BookName = m.BookName,
                    NoteForAll = m.NoteForAll,
                    SerialNumber = m.SerialNumber,
                    Quantity = 1,
                    StatusCode = nameof(DocStatus.Pending)
                });
                // nếu chọn lại từ serial -> xóa đi add lại
                if (IsChoseSerial && SelectedBODetails != null && SelectedBODetails.Any())
                {
                    ListBODetails.Remove(SelectedBODetails.First());
                }
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

        /// <summary>
        /// chọn nhân viên
        /// </summary>
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

        /// <summary>
        /// click vào dòng chọn sách -> hiển thị danh sách serial
        /// </summary>
        /// <param name="args"></param>
        protected async void OnRowClickBookHandler(GridRowClickEventArgs args)
        {
            try
            {
                var data = args.Item as BookModel;
                if (data == null) return;
                await ShowLoader();
                await getBookSerials(data.BookId, $"{data.BookName}");
                StateHasChanged();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "DocumentController", "OnRowClickBookHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await Task.Delay(100);
                await ShowLoader(false);
                await InvokeAsync(StateHasChanged);
            }
        }    

        protected async void SaveDocHandler(EnumType pProcess = EnumType.Update)
        {
            try
            {
                string strAction = nameof(EnumType.Add);
                string message = string.Empty;
                DocumentUpdate.StatusCode = nameof(DocStatus.Pending);
                // Lưu thông tin
                if (pIsCreate)
                {
                    
                    message = "Bạn có chắc muốn Thêm mới thông tin phiếu mượn ?";
                }
                else if (pProcess == EnumType.SaveAndClose)
                {
                    // Lưu và đóng lệnh này
                    message = "Bạn có chắc muốn Lưu và đóng phiếu mượn. [Tình trạng Đang mượn] ?";
                    strAction = nameof(EnumType.Update);
                    DocumentUpdate.StatusCode = nameof(DocStatus.Borrowing);
                }
                else
                {
                    strAction = nameof(EnumType.Update);
                    message = "Bạn có chắc muốn Cập nhật thông tin phiếu mượn này ?";
                }

                if (!validateData(pIsCreate)) return;
                bool isConfirm = await _rDialogs!.ConfirmAsync($"{message}", "Thông báo");
                if (!isConfirm) return;
                await ShowLoader();
                
                bool isSuccess = await _documentService!.UpdateBorrowOrder(JsonConvert.SerializeObject(DocumentUpdate)
                    , JsonConvert.SerializeObject(ListBODetails), strAction, pUserId);

                if (isSuccess)
                {
                    if(pIsCreate)
                    {
                        // chuyển sang tab theo dõi
                        // back sang link theo dõi đơn hàng
                        Dictionary<string, string> pParams = new Dictionary<string, string>
                        {
                            { "pStatusId", $"{nameof(DocStatus.Pending)}"},
                        };
                        string key = EncryptHelper.Encrypt(JsonConvert.SerializeObject(pParams)); // mã hóa key
                        _navigationManager!.NavigateTo($"/document-list?key={key}");
                        return;
                    }
                    await showVoucher();
                }    
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "DocumentController", "SaveDocHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await Task.Delay(100);
                await ShowLoader(false);
                await InvokeAsync(StateHasChanged);
            }
        }
        
        /// <summary>
        /// Trả sách
        /// </summary>
        /// <param name="isAll">1 cuốn hay tất cả</param>
        protected async void ReturnBooksHandler(bool isAll = true, BODetailModel? pItem = null)
        {
            try
            {
                if(isAll)
                {
                    bool isConfirm = await _rDialogs!.ConfirmAsync($"Bạn có chắc muốn trả lại tất cả sách không ?", "Thông báo");
                    if (!isConfirm) return;
                    await ShowLoader();
                    bool isSuccess = await _documentService!.ReturnBooksAsync(JsonConvert.SerializeObject(DocumentUpdate)
                        , JsonConvert.SerializeObject(ListBODetails), nameof(DocStatus.All), pUserId);
                    if (isSuccess)
                    {
                        await showVoucher();
                    }
                }  
                else
                {
                    // neeus trả 1 cuốn
                    if(pItem!.StatusCode == nameof(DocStatus.Closed))
                    {
                        // Sách này đã được trả
                        ShowWarning("Sách đã được trả!!!");
                        return;
                    }
                    bool isConfirm = await _rDialogs!.ConfirmAsync($"Bạn có chắc muốn trả lại sách [{pItem.BookName}] không ?", "Thông báo");
                    if (!isConfirm) return;
                    await ShowLoader();
                    var lst = new List<BODetailModel>();
                    lst.Add(pItem);
                    bool isSuccess = await _documentService!.ReturnBooksAsync(JsonConvert.SerializeObject(DocumentUpdate)
                        , JsonConvert.SerializeObject(lst), "Single", pUserId);
                    if (isSuccess)
                    {
                        await showVoucher();
                    }

                }    
                
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "DocumentController", "ReturnBooksHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await Task.Delay(100);
                await ShowLoader(false);
                await InvokeAsync(StateHasChanged);
            }
        }    
        #endregion
    }
}
