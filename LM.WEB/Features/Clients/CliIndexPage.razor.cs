﻿using LM.Models;
using LM.WEB.Features.Controllers;
using LM.WEB.Services;
using Microsoft.AspNetCore.Components;

namespace LM.WEB.Features.Clients
{
    public partial class CliIndexPage
    {
        #region Dependency Injection
        [Inject] private ILogger<CliIndexPage>? _logger { get; init; }
        [Inject] private ICliMasterDataService? _masterDataService { get; init; }
        [Inject] public ToastService? _toastService { get; init; }
        [Inject] public LoaderService? _loaderService { get; init; }
        #endregion
        public List<KindBookModel>? ListKindBooks { get; set; }
        public List<PublisherModel>? ListPublishers { get; set; }
        public List<AuthorModel>? ListAuthors{get; set;}
        public List<BookModel>? ListBooks { get; set; }

        public SearchModel ItemSearch = new SearchModel();

        #region Private Functions
        private async Task getDataBooks()
        {
            ListBooks = new List<BookModel>();
            ListBooks = await _masterDataService!.GetDataBooksAsync(ItemSearch);
        }

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
                    await ShowLoader();
                    await getDataCombo();
                    await getDataBooks();
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
    }
}