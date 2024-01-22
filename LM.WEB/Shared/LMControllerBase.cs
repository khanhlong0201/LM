using LM.Models.Shared;
using LM.WEB.Models;
using LM.WEB.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace LM.WEB.Shared;

public class LMControllerBase : ComponentBase
{
    #region Dependency Injection
    [Inject] public IProgressService? _progressService { get; init; }
    [Inject] public LoaderService? _loaderService { get; init; }
    [Inject] public ToastService? _toastService { get; init; }
    [Inject] public IDateTimeService? _dateTimeService { get; init; }
    [Inject] AuthenticationStateProvider? _authenticationStateProvider { get; set; }
    #endregion

    #region Properties
    [CascadingParameter]
    public EventCallback<List<BreadcrumLModel>> NotifyBreadcrumb { get; set; }
    public List<BreadcrumLModel>? ListBreadcrumbs { get; set; }

    public int pUserId { get; set; }
    public bool pIsAdmin { get; set; }
    public string pBranchId { get; set; } = "";
    public string FullName { get; set; } = "";
    public string pBranchName { get; set; } = "";
    #endregion Properties

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        try
        {
            await _progressService!.Start(); 
        }
        catch (Exception) { }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if(firstRender)
        {
            try
            {
                var oUser = await((Providers.ApiAuthenticationStateProvider)_authenticationStateProvider!).GetAuthenticationStateAsync();
                if (oUser != null)
                {
                    pUserId = int.Parse(oUser.User.Claims.FirstOrDefault(m => m.Type == "UserId")?.Value + "");
                    pBranchId = oUser.User.Claims.FirstOrDefault(m => m.Type == "BranchId")?.Value + "";
                    pIsAdmin = oUser.User.Claims.FirstOrDefault(m => m.Type == "IsAdmin")?.Value?.ToUpper() == "TRUE";
                    FullName = oUser.User.Claims.FirstOrDefault(m => m.Type == "FullName")?.Value + "";
                    pBranchName = oUser.User.Claims.FirstOrDefault(m => m.Type == "BranchName")?.Value + "";
                }
            }
            catch (Exception) { }
        }    
    }

    #region Public Functions
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

    public void ShowError(string pMessage, int pCloseAfter = 5500) => _toastService!.ShowError(pMessage, pCloseAfter);
    public void ShowWarning(string pMessage, int pCloseAfter = 5500) => _toastService!.ShowWarning(pMessage, pCloseAfter);
    public void ShowInfo(string pMessage, int pCloseAfter = 5500) => _toastService!.ShowInfo(pMessage, pCloseAfter);
    public void ShowSuccess(string pMessage, int pCloseAfter = 5500) => _toastService!.ShowSuccess(pMessage, pCloseAfter);
    #endregion
}
