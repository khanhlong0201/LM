using LM.Models;
using LM.Models.Shared;
using LM.WEB.Components;
using LM.WEB.Features.Pages;
using LM.WEB.Models;
using LM.WEB.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace LM.WEB.Shared;
public partial class MainLayout
{
    [Inject] private NavigationManager? _navManager { get; init; }
    [Inject] AuthenticationStateProvider? _authenticationStateProvider { get; set; }
    [Inject] ToastService? _toastService { get; init; }
    [Inject] private ILogger<MainLayout>? _logger { get; init; }
    [Inject] private LoaderService? _loaderService { get; init; }
    [Inject] private ICliMasterDataService? _masterDataService { get; init; }
    public List<BreadcrumLModel>? ListBreadcrumbs { get; set; }
    public string PageActive { get; set; } = "trang-chu";
    private bool IsShowDialogProfile { get; set; }
    public HConfirm? _rDialogProfiles { get; set; }
    public EditContext? _EditProfileContext { get; set; }
    public string FullName { get; set; } = "";
    public string RoleName { get; set; } = "";
    public string EmpNo { get; set; } = "";
    public string UserName { get; set; } = "";
    public int UserId { get; set; } = -1;
    public UserProfileModel UserUpdate { get; set; } = new UserProfileModel();
    EventCallback<List<BreadcrumLModel>> BreadcrumbsHandler =>
        EventCallback.Factory.Create(this, (Action<List<BreadcrumLModel>>)NotifyBreadcrumb);

    private void NotifyBreadcrumb(List<BreadcrumLModel> _breadcrumbs )
    {
        try
        {
            ListBreadcrumbs = _breadcrumbs;
            var uri = _navManager!.ToAbsoluteUri(_navManager.Uri);
            PageActive = uri.AbsolutePath;
            StateHasChanged();
        }
        catch (Exception) { }
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
    protected void OnOpenDialogHandler()
    {
        try
        {
            UserUpdate = new UserProfileModel();
            IsShowDialogProfile = true;
            UserUpdate.Id = UserId;
            UserUpdate.FullName = FullName;
            UserUpdate.EmpNo = EmpNo;
            UserUpdate.UserName = UserName;
            _EditProfileContext = new EditContext(UserUpdate);
        }
        catch (Exception ex)
        {
            _logger!.LogError(ex, "UserController", "OnOpenDialogHandler");
            _toastService!.ShowError(ex.Message);
        }
    }

    protected async void SaveDataHandler(EnumType pEnum = EnumType.SaveAndClose)
    {
        try
        {
            string sAction = nameof(EnumType.ChangePassWord);
            var checkData = _EditProfileContext!.Validate();
            if (!checkData) return;
            if (UserUpdate.PasswordNew + "" != UserUpdate.ReEnterPasswordNew + "")
            {
                _toastService!.ShowWarning("Nhập lại mật khẩu mới không đúng so với mật khẩu mới! Vui lòng nhập lại.");
                return;
            }
            await ShowLoader();
            List<UserModel>? listUsers =  await _masterDataService!.GetDataUsersAsync(UserId);
            if (listUsers != null && listUsers.Any())
            {
                var passWordCurrent = listUsers.First().Password;
                if (UserUpdate.Password != EncryptHelper.Decrypt(passWordCurrent + ""))
                {
                    _toastService!.ShowWarning("Mật khẩu hiện tại không đúng! Vui lòng nhập lại");
                    await ShowLoader(false);
                    return;
                }
            }
            bool isSuccess = await _masterDataService!.UpdateUserAsync(JsonConvert.SerializeObject(UserUpdate), sAction, UserId);
            if (isSuccess)
            {
                IsShowDialogProfile = false;
                return;
            }
        }
        catch (Exception ex)
        {
            _logger!.LogError(ex, "UserController", "SaveDataHandler");
            _toastService!.ShowError(ex.Message);
        }
        finally
        {
            await ShowLoader(false);
            await InvokeAsync(StateHasChanged);
        }
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var oUser = await ((Providers.ApiAuthenticationStateProvider)_authenticationStateProvider!).GetAuthenticationStateAsync();
            if (oUser != null)
            {
                FullName = oUser.User.Claims.FirstOrDefault(m => m.Type == "FullName")?.Value + "";
                bool isAdmin = oUser.User.Claims.FirstOrDefault(m => m.Type == "IsAdmin")?.Value?.ToUpper() == "TRUE";
                EmpNo = oUser.User.Claims.FirstOrDefault(m => m.Type == "EmpNo")?.Value + "";
                UserName = oUser.User.Claims.FirstOrDefault(m => m.Type == "UserName")?.Value + "";
                UserId  = int.Parse(oUser.User.Claims.FirstOrDefault(m => m.Type == "UserId")?.Value + "");
                RoleName = isAdmin ? $"{EmpNo} - Admin" : $"{EmpNo} - Nhân viên";
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (Exception) { }
    }

    
}