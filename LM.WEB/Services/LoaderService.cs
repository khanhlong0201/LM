using Microsoft.JSInterop;

namespace LM.WEB.Services;
public class LoaderService
{
    public event Action<bool>? OnShow;
    public void ShowLoader(bool pIsLoading = true) => OnShow!.Invoke(pIsLoading);
}

public interface IProgressService
{
    Task Start();
    Task SetPercent(double pPercent = 0.4);
    Task Done();
}

public class ProgressService : IProgressService
{
    private IJSRuntime _jsRuntime { get; set; }
    public ProgressService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    public async Task Done()
    {
        await _jsRuntime!.InvokeVoidAsync("NProgress.inc");
        await _jsRuntime!.InvokeVoidAsync("NProgress.done");
    }
    public async Task SetPercent(double pPercent = 0.4) => await _jsRuntime!.InvokeVoidAsync("NProgress.set", pPercent);
    public async Task Start() => await _jsRuntime!.InvokeVoidAsync("NProgress.start");
}

public enum ToastLevel
{
    Info,
    Success,
    Warning,
    Error
}

public class ToastService
{
    public event Action<string, int>? OnShowError;
    public event Action<string, int>? OnShowWarning;
    public event Action<string, int>? OnShowInfo;
    public event Action<string, int>? OnShowSuccess;
    public event Action<ToastLevel>? OnClear;
    public event Action? OnClearAll;
    public void ShowError(string message, int CloseAfter = 5500) => OnShowError?.Invoke(message, CloseAfter);
    public void ShowWarning(string message, int CloseAfter = 5500) => OnShowWarning?.Invoke(message, CloseAfter);
    public void ShowInfo(string message, int CloseAfter = 5500) => OnShowInfo?.Invoke(message, CloseAfter);
    public void ShowSuccess(string message, int CloseAfter = 5500) => OnShowSuccess?.Invoke(message, CloseAfter);
    public void ClearToast(ToastLevel level) => OnClear?.Invoke(level);
    public void ClearAll() => OnClearAll?.Invoke();
}

public class LoginDialogService
{
    public event Action<bool>? OnShow;
    public void ShowDialog(bool pIsShow = true) => OnShow!.Invoke(pIsShow);
}    

