using LM.Models;
using LM.Models.Shared;
using LM.WEB.Commons;
using LM.WEB.Components;
using LM.WEB.Models;
using LM.WEB.Services;
using LM.WEB.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Telerik.Blazor.Components;

namespace LM.WEB.Features.Controllers;
public class BookController : LMControllerBase
{
    #region Dependency Injection
    [Inject] private ILogger<KindBookController>? _logger { get; init; }
    [Inject] private ICliMasterDataService? _masterDataService { get; init; }
    [Inject] private IConfiguration? _configuration { get; init; }
    [Inject] private IWebHostEnvironment? _webHostEnvironment { get; init; }
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
    public List<AuthorModel>? ListAuthors { get; set; }
    public List<IBrowserFile> ListBrowserFiles { get; set; } = new();   // Danh sách file lưu tạm => Upload file
    public SearchModel ItemFilter { get; set; } = new SearchModel();

    public List<ImageModel>? ListImagesTemp { get; set; } // ds hình tạm -> khi chọn lưu vào đây'
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
        ListImagesTemp = new List<ImageModel>();
        ListBooks = await _masterDataService!.GetDataBooksAsync(ItemFilter);
    }

    private async Task getDataCombo()
    {
        ListKindBooks = new List<KindBookModel>();
        ListPublishers = new List<PublisherModel>();
        ListKindBooks = await _masterDataService!.GetDataKindBooksAsync();
        ListPublishers = await _masterDataService!.GetDataPublishersAsync();
        ListAuthors = await _masterDataService!.GetAuthorsAsync();
    }

    #endregion

    #region Protected Functions

    /// <summary>
    /// load image lên để view
    /// </summary>
    /// <param name="args"></param>
    //protected async void OnLoadFileHandler(InputFileChangeEventArgs args)
    //{
    //    try
    //    {
    //        //await args.File.RequestImageFileAsync("image/*", 600, 600);
    //        ListImages = new List<ImageDetailModel>();
    //        if (ListBrowserFiles == null) ListBrowserFiles = new List<IBrowserFile>();
    //        ListBrowserFiles.AddRange(args.GetMultipleFiles());
    //        foreach (var item in args.GetMultipleFiles())
    //        {
    //            using Stream imageStream = item.OpenReadStream(long.MaxValue);
    //            using MemoryStream ms = new();
    //            //copy imageStream to Memory stream
    //            await imageStream.CopyToAsync(ms);
    //            //convert stream to base64
    //            ListImages.Add(new ImageDetailModel() { ImageUrl = $"data:image/png;base64,{Convert.ToBase64String(ms.ToArray())}" });
    //            await ms.FlushAsync();
    //            await ms.DisposeAsync();
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger!.LogError(ex, "OnLoadFileHandler");
    //        _toastService!.ShowError(ex.Message);
    //    }
    //    finally
    //    {
    //        await InvokeAsync(StateHasChanged);
    //    }
    //}

    protected async void OnLoadFileHandler(InputFileChangeEventArgs args)
    {
        try
        {
            if (args.FileCount <= 0) return;
            await ShowLoader();
            if (ListImagesTemp == null) ListImagesTemp = new List<ImageModel>();
            var rootFolder = Path.Combine(_webHostEnvironment!.WebRootPath, "Upload", "Temps");
            //tạo thư mục
            if (!Directory.Exists(rootFolder)) Directory.CreateDirectory(rootFolder);
            string strFileFullName = string.Empty;
            var file = args.GetMultipleFiles().First();
            string fileNameNew = $"{Guid.NewGuid()}---{file.Name}";
            strFileFullName = Path.Combine(rootFolder, fileNameNew);
            await using FileStream fs = new(strFileFullName, FileMode.Create);
            await file.OpenReadStream(long.MaxValue).CopyToAsync(fs);
            await fs.FlushAsync();
            await fs.DisposeAsync();
            ImageModel oItem = new ImageModel();
            oItem.FileName = fileNameNew;
            oItem.FilePath = strFileFullName;
            oItem.ImageUrl = $"../Upload/Temps/{fileNameNew}";
            oItem.IsDelete = false;
            oItem.IsAdd = true;
            // cập nhật nó là true để khỏi upload -> nhưng phải remove temp. ví dụ họ chọn mà không lưu
            foreach (var item in ListImagesTemp)
            {
                item.IsDelete = true;
            }
            ListImagesTemp.Add(oItem);
            BookUpdate.ImageUrl = $"../Upload/Temps/{fileNameNew}";
            BookUpdate.ImageUrlView = $"../Upload/Temps/{fileNameNew}";
        }
        catch (Exception ex)
        {
            _logger!.LogError(ex, "OnLoadFileHandler");
            _toastService!.ShowError(ex.Message);
        }
        finally
        {
            await Task.Delay(75);
            await ShowLoader(false);
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// khi tắt popup nếu đã chọn file rồi nhưng chưa up
    /// thì xóa đi trong temp
    /// </summary>
    protected void RemoveFileCallbackHandler()
    {
        try
        {
            var rootFolder = Path.Combine(_webHostEnvironment!.WebRootPath, "Upload", "Temps");
            if (Directory.Exists(rootFolder))
            {
                DirectoryInfo di = new DirectoryInfo(rootFolder);
                foreach (FileInfo file in di.GetFiles())
                {
                    // kiểm lớn hơn 5p xóa đi
                    if ((DateTime.Now - file.CreationTime).TotalMinutes > 5) file.Delete();
                }
            }
        }
        catch (Exception ex)
        {
            _logger!.LogError(ex, "OnLoadFileHandler");
            _toastService!.ShowError(ex.Message);
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

    protected void OnOpenDialogHandler(EnumType pAction = EnumType.Add, BookModel? pItemDetails = null)
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
                BookUpdate = new BookModel();
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
                BookUpdate.ImageUrl = pItemDetails.ImageUrl;
                BookUpdate.ImageUrlView = pItemDetails.ImageUrlView;
                BookUpdate.AuthorId = pItemDetails.AuthorId;
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

    protected async void SaveDataHandler(EnumType pEnum = EnumType.SaveAndClose)
    {
        try
        {
            string sAction = IsCreate ? nameof(EnumType.Add) : nameof(EnumType.Update);
            var checkData = _EditContext!.Validate();
            if (!checkData) return;
            await ShowLoader();
            // lưu hình ảnh
            if (ListImagesTemp != null && ListImagesTemp.Any())
            {
                var lstImages = await _masterDataService!.UploadImagesAsync(ListImagesTemp);
                if (lstImages != null && lstImages.Any())
                {
                    BookUpdate.ImageUrl = lstImages.First().FileName;
                }
            }
            bool isSuccess = await _masterDataService!.UpdateBookAsync(JsonConvert.SerializeObject(BookUpdate), sAction, pUserId);
            if (isSuccess)
            {
                await getDataBooks();
                IsShowDialog = false;
                return;
            }
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