using LM.Models;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using LM.Models;
using LM.Models.Shared;
using LM.WEB.Commons;
using LM.WEB.Components;
using LM.WEB.Models;
using LM.WEB.Services;
using LM.WEB.Shared;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.WebUtilities;

namespace LM.WEB.Features.Pages
{
    public partial class BookDetailClient
    {
        [Inject] private ILogger<IndexClient>? _logger { get; init; }
        [Inject] NavigationManager? _navigationManager { get; set; }
        [Inject] IConfiguration? _configuration { get; set; }
        [Inject] ILocalStorageService? _localStorage { get; set; }
        [Inject] private ICliMasterDataService? _masterDataService { get; init; }
        [Inject] public LoaderService? _loaderService { get; init; }
        [Inject] public ToastService? _toastService { get; init; }

        public bool IsShowDialog { get; set; }

        public int PageIndex = 1;
        public class CarouselModel
        {
            public string Url { get; set; } = string.Empty;
        }
        public List<CarouselModel> CarouselData { get; set; } = new List<CarouselModel>()
        {
            new CarouselModel() { Url = @"https://lighthouse.chotot.com/_next/image?url=https%3A%2F%2Fcdn.chotot.com%2Fadmincentre%2FICGqIPhBAn559vSI4v7jaBAYFYegeRG7xSfUJ6tkugI%2Fpreset%3Araw%2Fplain%2F6ec3994f81e14d768dfc467847ce430c-2820195948173896828.jpg&w=1080&q=90"},
            new CarouselModel() { Url = @"https://lighthouse.chotot.com/_next/image?url=https%3A%2F%2Fcdn.chotot.com%2Fadmincentre%2FICGqIPhBAn559vSI4v7jaBAYFYegeRG7xSfUJ6tkugI%2Fpreset%3Araw%2Fplain%2F6ec3994f81e14d768dfc467847ce430c-2820195948173896828.jpg&w=1080&q=90"},
            new CarouselModel() { Url = @"https://lighthouse.chotot.com/_next/image?url=https%3A%2F%2Fcdn.chotot.com%2Fadmincentre%2FICGqIPhBAn559vSI4v7jaBAYFYegeRG7xSfUJ6tkugI%2Fpreset%3Araw%2Fplain%2F6ec3994f81e14d768dfc467847ce430c-2820195948173896828.jpg&w=1080&q=90"},
            new CarouselModel() { Url = @"https://lighthouse.chotot.com/_next/image?url=https%3A%2F%2Fcdn.chotot.com%2Fadmincentre%2FICGqIPhBAn559vSI4v7jaBAYFYegeRG7xSfUJ6tkugI%2Fpreset%3Araw%2Fplain%2F6ec3994f81e14d768dfc467847ce430c-2820195948173896828.jpg&w=1080&q=90"},
        };

        public string binding { get; set; } = "";
        public List<string> ListData = new List<string>() { "Quận 1", "Quận 2", "Quận 3", "Quận 4", "Quận 5" };

        public BorrowingModel BorrowingUpdate { get; set; } = new BorrowingModel();
        public EditContext? _EditContext { get; set; }


        //[CascadingParameter(Name = "pUserId")]
        //private int pUserId { get; set; } // giá trị từ ClientLayout

        //[CascadingParameter(Name = "pIsSupperAdmin")]
        //private bool pIsSupperAdmin { get; set; } // giá trị từ MainLayout

        #region Properties
        public CliBookModel CliBook { get; set; } = new CliBookModel();
        public List<string> ListImages { get; set; } = new List<string>();
        public int pBookId { get; set; }
        public List<PublisherModel> ListPublisher { get; set; } = new List<PublisherModel>();
        public List<KindBookModel>? ListKindBook { get; set; } = new List<KindBookModel>();
        public List<BookModel>? ListPublishingYear { get; set; } = new List<BookModel>();
        public SearchModel Search { get; set; } = new SearchModel();
        #endregion

        #region Override Functions

        protected override Task OnInitializedAsync()
        {
            try
            {
                // đọc giá tri câu query
                var uri = _navigationManager?.ToAbsoluteUri(_navigationManager.Uri);
                if (uri != null)
                {
                    var queryStrings = QueryHelpers.ParseQuery(uri.Query);
                    if (queryStrings.Count() > 0)
                    {
                        string key = uri.Query.Substring(5); // để tránh parse lỗi;
                        pBookId = Convert.ToInt32(key);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "OnInitializedAsync");
            }
            return base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(firstRender)
            {
                try
                {
                    //await ShowLoader();
                    await getDataCombo();
                    await getDataDetail();
                }
                catch (Exception ex)
                {
                    _logger!.LogError(ex, "OnAfterRenderAsync");
                    _toastService!.ShowError(ex.Message);
                }
                finally
                {
                    //await ShowLoader(false);
                    await InvokeAsync(StateHasChanged);
                }
            }    
        }
        #endregion

        #region "Private Functions"
        /// <summary>
        /// loading
        /// </summary>
        /// <param name="isShow"></param>
        /// <returns></returns>
        public async Task ShowLoader(bool isShow = true)
        {
            if (isShow)
            {
                _loaderService!.ShowLoader(isShow);
                await Task.Yield();
                return;
            }
            _loaderService!.ShowLoader(isShow);
        }
        private async Task getDataDetail()
        {
            try
            {
                BorrowingUpdate = await _masterDataService!.GetDataBookDetailClientsAsync(pBookId);

            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "SaveDataHandler");
                _toastService!.ShowError(ex.Message);
            }
            finally
            {
                //await ShowLoader(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task getDataCombo()
        {
            ListPublisher = new List<PublisherModel>();
            ListPublisher = await _masterDataService!.GetDataPublishersAsync();
            ListKindBook = new List<KindBookModel>();
            ListKindBook = await _masterDataService!.GetDataKindBooksAsync();
        }

        #endregion

        #region "Protected Functions"
        protected void OpenPopupBookingHandler()
        {
            try
            {
                BorrowingUpdate = new BorrowingModel();
                _EditContext = new EditContext(BorrowingUpdate);
                IsShowDialog = true;
            }
            catch(Exception ex)
            {
                _logger!.LogError(ex, "OnOpenDialogHandler");
                _toastService!.ShowError(ex.Message);
            }
        }

        /// <summary>
        /// lưu dữ liệu booking
        /// </summary>
        protected async void SaveDataHandler()
        {
            try
            {
                var checkData = _EditContext!.Validate();
                if (!checkData) return;
                //await ShowLoader();
                BorrowingUpdate.BookId = pBookId;
                BorrowingUpdate.UserId = 1;
                BorrowingUpdate.Status = "Chờ xử lý";
                //RequestModel request = new RequestModel()
                //{
                //    Json = JsonConvert.SerializeObject(BorrowingUpdate),
                //    Type = nameof(EnumType.Add),
                //    UserId = pUserId > 0 ? pUserId : 1
                //};
                //string resString = await _apiService!.AddOrUpdateData(EndpointConstants.URL_BOOKING_UPDATE, request);
                //if (!string.IsNullOrEmpty(resString))
                //{
                //    _toastService!.ShowSuccess($"Đã lưu thông tin. Chủ phòng sẽ liên hệ lại sau!!!");
                //    IsShowDialog = false;
                //}
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "SaveDataHandler");
                _toastService!.ShowError(ex.Message);
            }
            finally
            {
                //await ShowLoader(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async void OnChangePublisherHandler(int iPublisher)
        {
            Search.PublisherId = iPublisher;
            //await getDistrictByCity(iCityId);
        }

        protected async void OnChangeKindBookHandler(int iKindBoook)
        {
            Search.KindBookId = iKindBoook;
            //await getWardByDistrict(iDistinctId);
        }

        protected async void OnChangePublishingYearHandler(int iPublishingYear)
        {
            Search.PublishingYear = iPublishingYear;
            //await getWardByDistrict(iDistinctId);
        }

        protected async void ReLoadDataHandler()
        {
            try
            {
                Search.Limit = 5;
                Search.Page = 0;
                if (await _localStorage!.ContainKeyAsync("oFilter")) await _localStorage!.RemoveItemAsync("oFilter");
                await _localStorage!.SetItemAsync("oFilter", Search);
                _navigationManager!.NavigateTo("/trang-chu");
                //await getDataBHouse();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "ReLoadDataHandler");
                _toastService!.ShowError(ex.Message);
            }
            finally
            {
                await InvokeAsync(StateHasChanged);
            }
        }
        #endregion
    }
}
