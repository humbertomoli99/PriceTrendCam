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
public partial class AddSelectorsViewModel : ObservableRecipient, INavigationAware
{
    // TODO: Set the default URL to display.
    private Uri _source = new("https://docs.microsoft.com/windows/apps/");
    private bool _isLoading = true;
    private int _newstoreId;

    [ObservableProperty]
    private string typeDataComboBox;

    [ObservableProperty]
    private string selectorTextBox;

    public string selectedCssSelector
    {
        get; set;
    }

    [ObservableProperty]
    private string getAttributeComboBox;

    [ObservableProperty]
    private string regexTextBox;

    [ObservableProperty]
    private bool isNotNullCheckBox;

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
    public Store ObjectSelector
    {
        get;
        private set;
    }

    [ObservableProperty]
    private bool hasFailures;

    public AddSelectorsViewModel(IWebViewService webViewService)
    {
        WebViewService = webViewService;
    }
    [RelayCommand]
    private async Task OpenInBrowser()
    {
        if (WebViewService.Source != null)
        {
            await Windows.System.Launcher.LaunchUriAsync(WebViewService.Source);
        }
    }
    [RelayCommand]
    private void Reload()
    {
        WebViewService.Reload();
    }
    [RelayCommand]
    private void BrowserForward()
    {
        if (WebViewService.CanGoForward)
        {
            WebViewService.GoForward();
        }
    }
    [RelayCommand]
    private void BrowserBack()
    {
        if (WebViewService.CanGoBack)
        {
            WebViewService.GoBack();
        }
    }
    [RelayCommand]
    private async void SaveSelectors()
    {
        // Obtener la instancia de Store que deseas actualizar
        var store = await App.PriceTrackerService.GetWithChildrenAsync<Store>(_newstoreId);

        // Crear lista de Selectors
        var newSelector = new Selector
        {
            CssSelector = selectedCssSelector,
            Type = typeDataComboBox,
            StoreId = store.Id,
            Store = store
        };

        // Crear objeto Store y guardarlo en la base de datos

        await App.PriceTrackerService.InsertAsync<Selector>(newSelector);


        //Debug.WriteLine(typeDataComboBox);
        //Debug.WriteLine(selectorTextBox);
        //Debug.WriteLine(getAttributeComboBox);
        //Debug.WriteLine(regexTextBox);
        //Debug.WriteLine(isNotNullCheckBox);
        //Debug.WriteLine(_newstoreId);
        //Debug.WriteLine(selectedCssSelector);
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
        OnPropertyChanged(nameof(BrowserBack));
        OnPropertyChanged(nameof(BrowserForward));
        if (webErrorStatus != default)
        {
            hasFailures = true;
        }
    }

    [RelayCommand]
    private void OnRetry()
    {
        hasFailures = false;
        IsLoading = true;
        WebViewService?.Reload();
    }
}
