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
public class BookController : LMControllerBase
{
    #region Dependency Injection
    [Inject] private ILogger<KindBookController>? _logger { get; init; }
    [Inject] private ICliMasterDataService? _masterDataService { get; init; }
    [Inject] private IConfiguration? _configuration { get; set; }
    #endregion

    #region Properties
    public bool IsInitialDataLoadComplete { get; set; } = true;
    public List<BookModel>? ListBooks { get; set; }
    public IEnumerable<BookModel>? SelectedBooks { get; set; } = new List<BookModel>();
    public BookModel BookUpdate { get; set; } = new BookModel();
    public EditContext? _EditContext { get; set; }
    public bool IsShowDialog { get; set; }
    public bool IsCreate { get; set; } = true;
    public HConfirm? _rDialogs { get; set; }
    public List<KindBookModel>? ListKindBooks { get; set; }
    public List<PublisherModel>? ListPublishers { get; set; }
    public List<IBrowserFile> ListBrowserFiles { get; set; } = new();   // Danh sách file lưu tạm => Upload file
    public List<ImageDetailModel> ListImages = new List<ImageDetailModel>();
    public SearchModel ItemFilter { get; set; } = new SearchModel();
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
                    new BreadcrumLModel() { Text = "Sách" }
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
                await getDataBooks();
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
    private async Task getDataBooks()
    {
        ListBooks = new List<BookModel>();
        SelectedBooks = new List<BookModel>();
        ListBooks = await _masterDataService!.GetDataBooksAsync(ItemFilter);
    }

    private async Task getDataCombo()
    {
        ListKindBooks = new List<KindBookModel>();
        ListPublishers = new List<PublisherModel>();
        ListKindBooks = await _masterDataService!.GetDataKindBooksAsync();
        ListPublishers = await _masterDataService!.GetDataPublishersAsync();
    }

    #endregion

    #region Protected Functions

    /// <summary>
    /// load image lên để view
    /// </summary>
    /// <param name="args"></param>
    protected async void OnLoadFileHandler(InputFileChangeEventArgs args)
    {
        try
        {
            //await args.File.RequestImageFileAsync("image/*", 600, 600);
            ListImages = new List<ImageDetailModel>();
            if (ListBrowserFiles == null) ListBrowserFiles = new List<IBrowserFile>();
            ListBrowserFiles.AddRange(args.GetMultipleFiles());
            foreach (var item in args.GetMultipleFiles())
            {
                using Stream imageStream = item.OpenReadStream(long.MaxValue);
                using MemoryStream ms = new();
                //copy imageStream to Memory stream
                await imageStream.CopyToAsync(ms);
                //convert stream to base64
                ListImages.Add(new ImageDetailModel() { ImageUrl = $"data:image/png;base64,{Convert.ToBase64String(ms.ToArray())}" });
                await ms.FlushAsync();
                await ms.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            _logger!.LogError(ex, "OnLoadFileHandler");
            _toastService!.ShowError(ex.Message);
        }
        finally
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    protected async void ReLoadDataHandler()
    {
        try
        {
            IsInitialDataLoadComplete = false;
            await getDataBooks();
        }
        catch (Exception ex)
        {
            _logger!.LogError(ex, "BookController", "ReLoadDataHandler");
            ShowError(ex.Message);
        }
        finally
        {
            IsInitialDataLoadComplete = true;
            await InvokeAsync(StateHasChanged);
        }
    }

    protected async Task OnOpenDialogHandler(EnumType pAction = EnumType.Add, BookModel? pItemDetails = null)
    {
        try
        {
            if (pAction == EnumType.Add)
            {
                IsCreate = true;
                BookUpdate = new BookModel();
            }
            else
            {
                BookUpdate.BookId = pItemDetails!.BookId;
                BookUpdate.BookName = pItemDetails!.BookName;
                BookUpdate.Language = pItemDetails!.Language;
                BookUpdate.NumOfPage = pItemDetails!.NumOfPage;
                BookUpdate.PublishingYear = pItemDetails!.PublishingYear;
                BookUpdate.FilePath = pItemDetails!.FilePath;
                BookUpdate.ImageId = pItemDetails!.ImageId;
                BookUpdate.PublisherId = pItemDetails!.PublisherId;
                BookUpdate.KindBookId = pItemDetails!.KindBookId;
                BookUpdate.Description = pItemDetails.Description;
                BookUpdate.DateCreate = pItemDetails.DateCreate;
                BookUpdate.UserCreate = pItemDetails.UserCreate;
                ListImages = await _masterDataService!.GetDataImageDetailsAsync(BookUpdate.ImageId.Value);
                string url = _configuration!.GetSection("appSettings:ApiUrl").Value + DefaultConstants.FOLDER_BOOK + "/"; ;
                ListImages = ListImages.Select(m => new ImageDetailModel() { ImageUrl = url + m.ImageUrl, ImageDetailId = m.ImageDetailId, ImageId = m.ImageId, FileName = m.ImageUrl }).ToList();
                BookUpdate.ListFile = ListImages;
                IsCreate = false;
            }
            IsShowDialog = true;
            _EditContext = new EditContext(BookUpdate);
        }
        catch (Exception ex)
        {
            _logger!.LogError(ex, "BookController", "OnOpenDialogHandler");
            ShowError(ex.Message);
        }
    }

    //protected async void SaveDataHandler(EnumType pEnum = EnumType.SaveAndClose)
    //{
    //    try
    //    {
    //        string sAction = IsCreate ? nameof(EnumType.Add) : nameof(EnumType.Update);
    //        var checkData = _EditContext!.Validate();
    //        if (!checkData) return;
    //        await ShowLoader();
    //        bool isSuccess = await _masterDataService!.UpdateBookAsync(JsonConvert.SerializeObject(BookUpdate), sAction, pUserId);
    //        if (isSuccess)
    //        {

    //            //async Task Action()
    //            //{
    //            //    bool isSuccess = await _masterDataService!.UpdateBookAsync(JsonConvert.SerializeObject(BookUpdate), sAction, pUserId);
    //            //}
    //            //if (sAction == nameof(EnumType.Add))
    //            //{
    //            //    if (ListBrowserFiles == null || !ListBrowserFiles.Any())
    //            //    {
    //            //        // kiểm tra file
    //            //        _toastService!.ShowWarning("Vui lòng chọn hình ảnh cho phòng");
    //            //        return;
    //            //    }
    //            //    // lưu file -> nhả lên các 
    //            //    string resStringFile = await _masterDataService!.UploadMultiFiles($"Images/UploadImages?subFolder={DefaultConstants.FOLDER_BOOK}", ListBrowserFiles);
    //            //    if (!string.IsNullOrEmpty(resStringFile))
    //            //    {
    //            //        BookUpdate.ListFile = JsonConvert.DeserializeObject<List<ImageDetailModel>>(resStringFile);
    //            //        await Action();
    //            //    }
    //            //}
    //            //else
    //            //{
    //            //    BookUpdate.ListFile = ListImages;
    //            //    if (BookUpdate.ListFile == null) BookUpdate.ListFile = new List<ImageDetailModel>();
    //            //    // nếu có file đính kèm ->
    //            //    if (ListBrowserFiles != null && ListBrowserFiles.Any())
    //            //    {
    //            //        // lưu file -> nhả lên các 
    //            //        string resStringFile = await _masterDataService!.UploadMultiFiles($"Images/UploadImages?subFolder={DefaultConstants.FOLDER_BOOK}", ListBrowserFiles);
    //            //        if (!string.IsNullOrEmpty(resStringFile))
    //            //        {
    //            //            if (BookUpdate.ListFile == null) BookUpdate.ListFile = new List<ImageDetailModel>();
    //            //            BookUpdate.ListFile.AddRange(JsonConvert.DeserializeObject<List<ImageDetailModel>>(resStringFile));
    //            //            await Action();
    //            //        }
    //            //        return;
    //            //    }
    //            //    await Action();
    //            //}

    //            await getDataBooks();
    //            IsShowDialog = false;
    //            return;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger!.LogError(ex, "BookController", "SaveDataHandler");
    //        ShowError(ex.Message);
    //    }
    //    finally
    //    {
    //        await ShowLoader(false);
    //        await InvokeAsync(StateHasChanged);
    //    }
    //}

    protected async void SaveDataHandler(EnumType pEnum = EnumType.SaveAndClose)
    {
        try
        {
            string sAction = IsCreate ? nameof(EnumType.Add) : nameof(EnumType.Update);
            var checkData = _EditContext!.Validate();
            if (!checkData) return;

            if (BookUpdate != null && BookUpdate.PublisherId + "" == "")
            {
                _toastService.ShowWarning("Bạn phải chọn nhà xuất bản !");
                return;
            }
            if (BookUpdate.KindBookId + "" == "")
            {
                _toastService.ShowWarning("Bạn phải chọn loại sách !");
                return;
            }
            if (BookUpdate.Description + "" == "")
            {
                _toastService.ShowWarning("Bạn phải chọn miêu tả !");
                return;
            }
            if (BookUpdate.PublishingYear == 0 )
            {
                _toastService.ShowWarning("Bạn phải nhập năm xuất bản !");
                return;
            }
            await ShowLoader();

            async Task Action()
            {
                bool isSuccess = await _masterDataService!.UpdateBookAsync(JsonConvert.SerializeObject(BookUpdate), sAction, pUserId);
                if (isSuccess)
                {
                    ListImages = new List<ImageDetailModel>();
                    ListBrowserFiles = new List<IBrowserFile>();
                }
            }
            if (sAction == nameof(EnumType.Add))
            {
                if (ListBrowserFiles == null || !ListBrowserFiles.Any())
                {
                    // kiểm tra file
                    _toastService!.ShowWarning("Vui lòng chọn hình ảnh cho sách");
                    return;
                }
                // lưu file -> nhả lên các 
                string resStringFile = await _masterDataService!.UploadMultiFiles($"MasterData/UploadImages?subFolder={DefaultConstants.FOLDER_BOOK}", ListBrowserFiles);
                if (!string.IsNullOrEmpty(resStringFile))
                {
                    BookUpdate.ListFile = JsonConvert.DeserializeObject<List<ImageDetailModel>>(resStringFile);
                    await Action();
                }
            }
            else
            {
                BookUpdate.ListFile = ListImages;
                if (BookUpdate.ListFile == null) BookUpdate.ListFile = new List<ImageDetailModel>();
                // nếu có file đính kèm ->
                if (ListBrowserFiles != null && ListBrowserFiles.Any())
                {
                    // lưu file -> nhả lên các 
                    string resStringFile = await _masterDataService!.UploadMultiFiles($"MasterData/UploadImages?subFolder={DefaultConstants.FOLDER_BOOK}", ListBrowserFiles);
                    if (!string.IsNullOrEmpty(resStringFile))
                    {
                        if (BookUpdate.ListFile == null) BookUpdate.ListFile = new List<ImageDetailModel>();
                        BookUpdate.ListFile.AddRange(JsonConvert.DeserializeObject<List<ImageDetailModel>>(resStringFile));
                        await Action();
                    }
                    return;
                }
                await Action();
            }

            await getDataBooks();
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
    protected void OnRowDoubleClickHandler(GridRowClickEventArgs args) => OnOpenDialogHandler(EnumType.Update, args.Item as BookModel);

    protected async void DeleteDataHandler()
    {
        try
        {
            if (SelectedBooks == null || !SelectedBooks.Any())
            {
                ShowWarning(DefaultConstants.MESSAGE_NO_CHOSE_DATA);
                return;
            }
            var confirm = await _rDialogs!.ConfirmAsync($" {DefaultConstants.MESSAGE_CONFIRM_DELETE} ");
            if (!confirm) return;
            await ShowLoader();
            bool isSuccess = await _masterDataService!.DeleteDataAsync(nameof(EnumTable.Books), "", string.Join(",", SelectedBooks.Select(m => m.BookId)), pUserId);
            if (isSuccess)
            {
                await getDataBooks();
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