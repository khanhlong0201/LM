using Blazored.LocalStorage;
using LM.Models;
using LM.WEB.Features.Controllers;
using LM.WEB.Models;
using LM.WEB.Services;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Telerik.Blazor.Components.Common.Grid.Interfaces;

namespace LM.WEB.Features.Clients
{
    public partial class CliIndexPage
    {
        #region Dependency Injection
        [Inject] private ILogger<CliIndexPage>? _logger { get; init; }
        [Inject] private ICliMasterDataService? _masterDataService { get; init; }
        [Inject] public ToastService? _toastService { get; init; }
        [Inject] public LoaderService? _loaderService { get; init; }
        [Inject] private LoginDialogService? _bhDialogService { get; init; }
        [Inject] private ILocalStorageService? _localStorage { get; init; }
        #endregion
        public List<KindBookModel>? ListKindBooks { get; set; }
        public List<PublisherModel>? ListPublishers { get; set; }
        public List<AuthorModel>? ListAuthors{get; set;}
        public List<BookModel>? ListBooks { get; set; }

        public SearchModel ItemSearch = new SearchModel();
        public PaginationModel Pagination { get; set; } = new PaginationModel();

        [CascadingParameter]
        public EventCallback<BookModel> NotifyBook { get; set; }

        private const int PAGE_SIZE = 4;
        #region Private Functions

        private async Task getDataCombo()
        {
            ListKindBooks = new List<KindBookModel>();
            ListPublishers = new List<PublisherModel>();
            ListAuthors = new List<AuthorModel>();
            ListKindBooks = await _masterDataService!.GetDataKindBooksAsync();
            ListPublishers = await _masterDataService!.GetDataPublishersAsync();
            ListAuthors = await _masterDataService!.GetAuthorsAsync();
        }

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

        #endregion

        #region Override Functions
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    ItemSearch.Limit = PAGE_SIZE;
                    ItemSearch.Page = 1;
                    ItemSearch.IsShowPagination = true;
                    await ShowLoader();
                    await getDataCombo();
                    await GetDataBooks();
                }
                catch (Exception ex)
                {
                    _logger!.LogError(ex, "OnAfterRenderAsync");
                    _toastService!.ShowError(ex.Message);
                }
                finally
                {
                    await Task.Delay(100);
                    await ShowLoader(false);
                    await InvokeAsync(StateHasChanged);
                }
            }
        }
        #endregion

        #region Protected Functions
        public async Task GetDataBooks(bool isShowLoading = false)
        {
            try
            {
                if(isShowLoading) await ShowLoader();  
                ListBooks = new List<BookModel>();
                ListBooks = await _masterDataService!.GetDataBooksAsync(ItemSearch);
                if (ListBooks == null || ListBooks.Count == 0)
                {
                    _toastService!.ShowWarning("Không có dữ liệu");
                    return;
                }
                Pagination = JsonConvert.DeserializeObject<PaginationModel>(ListBooks.First().JPagination + "") ?? new PaginationModel();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "OnAfterRenderAsync");
                throw;
            }
            finally
            {
                if (isShowLoading)
                {
                    await Task.Delay(150);
                    await ShowLoader(false);
                    await InvokeAsync(StateHasChanged);
                }    
                
            }

        }
        protected async Task AddToCartHandler(BookModel oItem)
        {
            try
            {
                if (oItem == null) return;
                // kiểm tra đăng nhập -> bắt đăng nhập
                var oData = await _localStorage!.GetItemAsync<LoginResponseViewModel>("authCliToken");
                if (oData == null)
                {
                    _bhDialogService!.ShowDialog();
                    return;
                }
                // thêm vào giỏ hàng
                await NotifyBook.InvokeAsync(oItem);
                _toastService!.ShowSuccess("Đã thêm sách vào giỏ hàng");
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "OnAfterRenderAsync");
                _toastService!.ShowError(ex.Message);
            }
            finally
            {
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async void OnChangePageIndex(int pageIndex, bool isNext = false)
        {
            try
            {
                await ShowLoader();
                ItemSearch.Limit = PAGE_SIZE;
                ItemSearch.Page = pageIndex;
                await GetDataBooks(true);
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "ReLoadDataHandler");
                _toastService!.ShowError(ex.Message);
            }
        }
        #endregion 
    }
}
