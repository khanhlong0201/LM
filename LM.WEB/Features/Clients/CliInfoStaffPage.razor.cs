using Blazored.LocalStorage;
using LM.Models;
using LM.WEB.Models;
using LM.WEB.Services;
using Microsoft.AspNetCore.Components;

namespace LM.WEB.Features.Clients
{
    public partial class CliInfoStaffPage
    {
        #region Dependency Injection
        [Inject] private ILogger<CliInfoStaffPage>? _logger { get; init; }
        [Inject] private ICliMasterDataService? _masterDataService { get; init; }
        [Inject] public ToastService? _toastService { get; init; }
        [Inject] public LoaderService? _loaderService { get; init; }
        [Inject] private LoginDialogService? _bhDialogService { get; init; }
        [Inject] private ILocalStorageService? _localStorage { get; init; }
        [Inject] private ICliDocumentService? _documentService { get; init; }
        #endregion

        public List<BorrowOrderModel>? ListDocuments { get; set; }
        public StaffModel StaffCurrent { get; set; } = new StaffModel();

        #region Private Functions

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

        private async Task getDataDocuments(string pStaffCode)
        {
            ListDocuments = new List<BorrowOrderModel>();
            StaffCurrent = new StaffModel();
            ListDocuments = await _documentService!.GetDocumentByStaffAsync(pStaffCode);
            if(ListDocuments != null && ListDocuments.Any())
            {
                var oHeader = ListDocuments[0];
                StaffCurrent.StaffCode = oHeader.StaffCode;
                StaffCurrent.FullName = oHeader.FullName;
                StaffCurrent.Department = oHeader.Department;
                StaffCurrent.StaffTypeName = oHeader.StaffTypeName;
                StaffCurrent.PhoneNumber = oHeader.PhoneNumber;
                StaffCurrent.Email = oHeader.Email;
                StaffCurrent.Address = oHeader.Address;
            }    


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
                    var oData = await _localStorage!.GetItemAsync<LoginResponseViewModel>("authCliToken");
                    if (oData != null)
                    {
                        await getDataDocuments(oData.StaffCode+"");
                    }
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
