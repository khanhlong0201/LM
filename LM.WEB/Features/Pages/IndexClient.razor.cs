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
namespace LM.WEB.Features.Pages
{
    public partial class IndexClient 
    {
        [Inject] private ILogger<IndexClient>? _logger { get; init; }
        [Inject] NavigationManager? _navigationManager { get; set; }
        [Inject] IConfiguration? _configuration { get; set; }
        [Inject] ILocalStorageService? _localStorage { get; set; }
        [Inject] private ICliMasterDataService? _masterDataService { get; init; }
        [Inject] public LoaderService? _loaderService { get; init; }
        [Inject] public ToastService? _toastService { get; init; }


        #region Properties Test
        public string binding { get; set; } = "";
        public int Page { get; set; } = 3;
        public int PageSize { get; set; } = 4;
        public int TotalCount { get; set; } = 50;
        #endregion

        public SearchModel Search { get; set; } = new SearchModel();
        public List<CliBookModel> ListBookClients = new List<CliBookModel>();
        public PaginationModel Pagination { get; set; } = new PaginationModel();
        public List<PublisherModel> ListPublisher { get; set; } = new List<PublisherModel>();
        public List<KindBookModel>? ListKindBook { get; set; } = new List<KindBookModel>();
        public List<BookModel>? ListPublishingYear { get; set; } = new List<BookModel>();

        public Dictionary<string, string> ListPrices = new Dictionary<string, string>(); //list giá
        public Dictionary<string, string> ListSize = new Dictionary<string, string>(); // list từ giá
        public List<ImageDetailModel> ListImages = new List<ImageDetailModel>();
        #region Override Functions

        protected override async Task OnInitializedAsync()
        {
            try
            {
                //await ShowLoader();
                Search.Limit = 5;
                Search.Page = 1;
                ListPrices.Add("1", "Dưới 1 triệu");
                ListPrices.Add("2", "Từ 1 - 3 triệu");
                ListPrices.Add("3", "Từ 3 - 5 triệu");
                ListPrices.Add("4", "Từ 5 - 7 triệu");
                ListPrices.Add("5", "Từ 7 - 10 triệu");
                ListPrices.Add("6", "Từ 10 - 15 triệu");
                ListPrices.Add("7", "Trên 15 triệu");

                ListSize.Add("1", "27cm x 24cm");
                ListSize.Add("2", "13cm x 19cm");
                ListSize.Add("3", "16cm x 24cm");
                ListSize.Add("4", "24cm x 16cm");
                ListSize.Add("5", "21cm x 24cm");
            }
            catch(Exception)
            {

            }
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(firstRender)
            {
                try
                {

                    await getDataBook();
                    await getDataCombo();
                }
                catch (Exception ex)
                {
                    _logger!.LogError(ex, "OnAfterRenderAsync");
                    _toastService!.ShowError(ex.Message);
                }
                finally
                {
                    await InvokeAsync(StateHasChanged);
                    //await ShowLoader(false);
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

        private async Task getDataCombo()
        {
            ListPublisher = new List<PublisherModel>();
            ListPublisher = await _masterDataService!.GetDataPublishersAsync();
            ListKindBook = new List<KindBookModel>();
            ListKindBook = await _masterDataService!.GetDataKindBooksAsync();
        }

        /// <summary>
        /// lấy danh sách sách
        /// </summary>
        /// <returns></returns>
        private async Task getDataBook()
        {
            ListBookClients = await _masterDataService!.GetDataBookClientsAsync(Search);
            foreach(var item in ListBookClients)
            {
                ListImages = new List<ImageDetailModel>();
                ListImages = await _masterDataService!.GetDataImageDetailsAsync(item.ImageId);
                string url = _configuration!.GetSection("appSettings:ApiUrl").Value + DefaultConstants.FOLDER_BOOK + "/"; ;
                ListImages = ListImages.Select(m => new ImageDetailModel() { ImageUrl = url + m.ImageUrl, ImageDetailId = m.ImageDetailId, ImageId = m.ImageId, FileName = m.ImageUrl }).ToList();
                item.ListImages = ListImages.Select(imageDetail => imageDetail.ImageUrl).ToList();
                item.ImageUrlBook = url + ListImages.FirstOrDefault().ImageUrl;
            }
        }

        #endregion

        #region "Protected Functions"
        protected async void OnChangePublisherHandler(int iPublisherId)
        {
            Search.PublisherId = iPublisherId;
            //await getDistrictByCity(iCityId);
        }

        protected async void OnChangeKindBookHandler(int iKindBook)
        {
            Search.KindBookId = iKindBook;
            //await getWardByDistrict(iDistinctId);
        }

        protected async void ReLoadDataHandler()
        {
            try
            {
                //await ShowLoader();
                Search.Limit = 5;
                Search.Page = 1;
                //await getDataBook();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "ReLoadDataHandler");
                _toastService!.ShowError(ex.Message);
            }
            finally
            {
                //await ShowLoader(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async void OnChangePageIndex(int pageIndex)
        {
            try
            {
                //await ShowLoader();
                Search.Limit = 5;
                Search.Page = pageIndex;
                //await getDataBook();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "ReLoadDataHandler");
                _toastService!.ShowError(ex.Message);
            }
            finally
            {
                //await ShowLoader(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async void OnChangePrice(string key, bool isPrice = true)
        {
            try
            {
                //await ShowLoader();
                Search.Limit = 5;
                Search.Page = 1;
                if (isPrice) Search.KeyPrice = Search.KeyPrice == key ? "" : key; 
                else Search.KeyAcreage = Search.KeyAcreage == key ? "" : key;
                await getDataBook();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "OnChangePrice");
                _toastService!.ShowError(ex.Message);
            }
            finally
            {
                //await ShowLoader(false);
                await InvokeAsync(StateHasChanged);
            }
        }
        #endregion

    }
}
