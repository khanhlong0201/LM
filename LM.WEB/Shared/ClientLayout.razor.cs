using LM.Models;
using LM.Models.Shared;
using LM.WEB.Services;
using LM.WEB.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace LM.WEB.Shared
{
    public partial class ClientLayout 
    {
        [Inject] AuthenticationStateProvider? _authenticationStateProvider { get; set; }
        [Inject] private ILogger<ClientLayout>? _logger { get; init; }
        [Inject] private ICliMasterDataService? _masterDataService { get; init; }
        [Inject] private IConfiguration? _configuration { get; set; }
        [Inject] NavigationManager? _navigationManager { get; set; }
        [Inject] public LoaderService? _loaderService { get; init; }
        public string breadcumb { get; set; } = "";
        public DateTime dt { get; set; } = DateTime.Now;
        public string FullName { get; set; } = "";
        public string UserName { get; set; } = "";
        public int UserId { get; set; } = -1;

        public List<BookModel>? ListBooks { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            try
            {
                var oUser = await ((WEB.Providers.ApiAuthenticationStateProvider)_authenticationStateProvider!).GetAuthenticationStateAsync();
                if (oUser != null)
                {
                    UserName = oUser.User.Claims.FirstOrDefault(m => m.Type == "UserName")?.Value + "";
                    FullName = oUser.User.Claims.FirstOrDefault(m => m.Type == "FullName")?.Value + "";
                    UserId = int.Parse(oUser.User.Claims.FirstOrDefault(m => m.Type == "UserId")?.Value + "");
                    //await showLoading(true);
                }
            }
            catch(Exception)
            {

            }
        }

        #region "Private Functions"
        EventCallback<BookModel> BooksHandler =>
        EventCallback.Factory.Create(this, (Action<BookModel>)NotifyBooks);
        private void NotifyBooks(BookModel _lstBooks)
        {
            if (ListBooks == null) ListBooks = new List<BookModel>();
            ListBooks.Add(_lstBooks);
        }

        #endregion
    }


}
