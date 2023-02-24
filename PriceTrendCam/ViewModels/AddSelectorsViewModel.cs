using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Web.WebView2.Core;

using PriceTrendCam.Contracts.Services;
using PriceTrendCam.Contracts.ViewModels;
using PriceTrendCam.Core.Models;

namespace PriceTrendCam.ViewModels;

// TODO: Review best practices and distribution guidelines for WebView2.
// https://docs.microsoft.com/microsoft-edge/webview2/get-started/winui
// https://docs.microsoft.com/microsoft-edge/webview2/concepts/developer-guide
// https://docs.microsoft.com/microsoft-edge/webview2/concepts/distribution
public class AddSelectorsViewModel : ObservableRecipient, INavigationAware
{
    // TODO: Set the default URL to display.
    private Uri _source = new("https://docs.microsoft.com/windows/apps/");
    private bool _isLoading = true;
    private bool _hasFailures;
    private int _newstoreId;


    public IWebViewService WebViewService
    {
        get;
    }

    public Uri Source
    {
        get => _source;
        set => SetProperty(ref _source, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool HasFailures
    {
        get => _hasFailures;
        set => SetProperty(ref _hasFailures, value);
    }

    public ICommand BrowserBackCommand
    {
        get;
    }

    public ICommand BrowserForwardCommand
    {
        get;
    }

    public ICommand ReloadCommand
    {
        get;
    }

    public ICommand RetryCommand
    {
        get;
    }

    public ICommand OpenInBrowserCommand
    {
        get;
    }

    public AddSelectorsViewModel(IWebViewService webViewService)
    {
        WebViewService = webViewService;

        BrowserBackCommand = new RelayCommand(() => WebViewService.GoBack(), () => WebViewService.CanGoBack);
        BrowserForwardCommand = new RelayCommand(() => WebViewService.GoForward(), () => WebViewService.CanGoForward);
        ReloadCommand = new RelayCommand(() => WebViewService.Reload());
        RetryCommand = new RelayCommand(OnRetry);
        OpenInBrowserCommand = new RelayCommand(async () => await Windows.System.Launcher.LaunchUriAsync(WebViewService.Source), () => WebViewService.Source != null);
    }

    public async Task OnNavigatedTo(object parameter)
    {
        if (parameter != null && parameter is int)
        {
            _newstoreId = (int)parameter;
            // Aquí puedes hacer algo con la variable _newstoreId, por ejemplo, asignarla a una propiedad del modelo de vista.
            var store = await App.PriceTrackerService.GetWithChildrenAsync<Store>(_newstoreId);
            var firstUrl = store.Urls.First().Url.ToString();
            Source = new Uri(firstUrl);
        }
        await Task.Run(() =>
        {
            WebViewService.NavigationCompleted += OnNavigationCompleted;
        });
    }


    public void OnNavigatedFrom()
    {
        WebViewService.UnregisterEvents();
        WebViewService.NavigationCompleted -= OnNavigationCompleted;
    }

    private void OnNavigationCompleted(object? sender, CoreWebView2WebErrorStatus webErrorStatus)
    {
        IsLoading = false;
        OnPropertyChanged(nameof(BrowserBackCommand));
        OnPropertyChanged(nameof(BrowserForwardCommand));
        if (webErrorStatus != default)
        {
            HasFailures = true;
        }
    }

    private void OnRetry()
    {
        HasFailures = false;
        IsLoading = true;
        WebViewService?.Reload();
    }
}
