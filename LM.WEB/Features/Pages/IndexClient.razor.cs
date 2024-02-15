using Blazored.LocalStorage;
using LM.WEB.Services;
using Microsoft.AspNetCore.Components;

namespace LM.WEB.Features.Pages
{
    public partial class IndexClient
    {
        [Inject] private ILogger<IndexClient>? _logger { get; init; }
        [Inject] private NavigationManager? _navigationManager { get; set; }
        [Inject] private IConfiguration? _configuration { get; set; }
        [Inject] private ILocalStorageService? _localStorage { get; set; }
        [Inject] private ICliMasterDataService? _masterDataService { get; init; }
        [Inject] public LoaderService? _loaderService { get; init; }
        [Inject] public ToastService? _toastService { get; init; }



        #region Override Functions

        protected override async Task OnInitializedAsync()
        {
            try
            {
            }
            catch (Exception)
            {
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
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

        #endregion Override Functions

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

        #endregion "Private Functions"
    }
}