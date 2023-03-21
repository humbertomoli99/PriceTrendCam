using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HtmlAgilityPack;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using PriceTrendCam.Contracts.Services;
using PriceTrendCam.Contracts.ViewModels;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Core.Services;

namespace PriceTrendCam.ViewModels;

// TODO: Review best practices and distribution guidelines for WebView2.
// https://docs.microsoft.com/microsoft-edge/webview2/get-started/winui
// https://docs.microsoft.com/microsoft-edge/webview2/concepts/developer-guide
// https://docs.microsoft.com/microsoft-edge/webview2/concepts/distribution
public partial class AddSelectorsViewModel : ObservableRecipient, INavigationAware
{
    // TODO: Set the default URL to display.
    private Uri? _source = new("https://pricetrendcam.blogspot.com/");
    private bool _isLoading = true;
    private int _newstoreId;

    [ObservableProperty]
    private string typeDataComboBox;

    [ObservableProperty]
    private string selectorTextBox;

    [ObservableProperty]
    private string getAttributeComboBox;

    [ObservableProperty]
    private string patternTextBox;

    [ObservableProperty]
    private string replacementTextBox;

    [ObservableProperty]
    private bool isNotNullCheckBox;

    [ObservableProperty]
    private bool hasFailures;
    public bool isRegistrationSuccessful;

    public IWebViewService WebViewService
    {
        get;
    }

    public Uri? Source
    {
        get => _source;
        set => SetProperty(ref _source, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string selectedCssSelector
    {
        get; set;
    }

    public Store GetStore
    {
        get;
        set;
    }
    public WebView2 webview
    {
        get;
        set;
    }
    //public List<Selector> GetListSelectors
    //{
    //    get;
    //    set;
    //}
    public StoreUrl GetUrls
    {
        get;
        set;
    }
    public ObservableCollection<Selector> GetListSelectors { get; set; } = new ObservableCollection<Selector>();
    public HtmlNode HtmlDocumentStore
    {
        get;
        set;
    }

    public AddSelectorsViewModel(IWebViewService webViewService)
    {
        WebViewService = webViewService;
        isRegistrationSuccessful = true;
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
            //CssSelector = selectedCssSelector,
            CssSelector = selectorTextBox,
            Command = selectorTextBox,
            Type = typeDataComboBox,
            Pattern = patternTextBox,
            Replacement = replacementTextBox,
            Attribute = getAttributeComboBox,
            StoreId = store.Id,
        };

        // Crear objeto Store y guardarlo en la base de datos

        isRegistrationSuccessful = Convert.ToBoolean(await App.PriceTrackerService.InsertAsync<Selector>(newSelector));
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
            await GetListSelectorsAsync();
            var firstUrl = GetStore.Urls.First().Url.ToString();

            await webview.EnsureCoreWebView2Async(); // Asegura que la instancia de CoreWebView2 esté inicializada.

            if (GetStore.DriveWebBrowser == WebBrowsers.HtmlAgilityPack)
            {

                webview.CoreWebView2.Settings.IsBuiltInErrorPageEnabled = true;
                webview.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                webview.CoreWebView2.Settings.IsZoomControlEnabled = true;
                webview.CoreWebView2.Settings.IsStatusBarEnabled = true;
                webview.CoreWebView2.Settings.AreDevToolsEnabled = true;
                webview.CoreWebView2.Settings.IsScriptEnabled = true;
                webview.CoreWebView2.Settings.AreHostObjectsAllowed = true;
                webview.CoreWebView2.Settings.IsWebMessageEnabled = true; // Habilita el modo sin conexión

                //WebViewService.Initialize(webview);

                HtmlDocumentStore = await HtmlDocumentService.LoadPageAsync(firstUrl);

                //TODO: Hacer que webview2 visualize el contenido real en un webview2 de htmlagibilitypack el node que descarga, el original
                //byte[] byteArray = Encoding.UTF8.GetBytes(htmlContent);
                //var stream = new MemoryStream(byteArray);

                //webview.CoreWebView2.NavigateToLocalStreamUri(stream.GetUri(), new CoreWebView2NavigationCompletedEventHandler((sender, args) =>
                //{
                //    if (args.IsSuccess)
                //    {
                //        // El archivo html se cargó correctamente
                //    }
                //    else
                //    {
                //        // Ocurrió un error al cargar el archivo html
                //    }
                //}));

                WebViewService.NavigateToString(HtmlDocumentStore.InnerHtml);
            }
            else
            {
                Source = new Uri(firstUrl);
            }
        }
        await Task.Run(() =>
        {
            WebViewService.NavigationCompleted += OnNavigationCompleted;
        });
    }

    public async Task GetListSelectorsAsync()
    {
        GetListSelectors.Clear();
        // Aquí puedes hacer algo con la variable _newstoreId, por ejemplo, asignarla a una propiedad del modelo de vista.
        GetStore = await App.PriceTrackerService.GetWithChildrenAsync<Store>(_newstoreId);

        foreach (var item in GetStore.Selectors)
        {
            GetListSelectors.Add(item);
        }
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
