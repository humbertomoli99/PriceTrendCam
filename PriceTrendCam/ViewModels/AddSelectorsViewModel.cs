using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HtmlAgilityPack;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
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
    public bool isSelectorDeleted;

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
    public List<RegexMethod> textBoxDataArray
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
    private async void DeleteSelector(Selector selector)
    {
        isSelectorDeleted = Convert.ToBoolean(await App.PriceTrackerService.DeleteAsync<Selector>(selector.Id));
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
    [RelayCommand(CanExecute = nameof(BrowserCanGoForward))]
    private void BrowserForward()
    {
        if (WebViewService.CanGoForward)
        {
            WebViewService.GoForward();
        }
    }
    private bool BrowserCanGoForward()
    {
        return WebViewService.CanGoForward;
    }
    [RelayCommand(CanExecute = nameof(BrowserCanGoBack))]
    private void BrowserBack()
    {
        if (WebViewService.CanGoBack)
        {
            WebViewService.GoBack();
        }
    }
    private bool BrowserCanGoBack()
    {
        return WebViewService.CanGoBack;
    }
    public async Task<bool> ValidateSelectorData()
    {
        var store = await App.PriceTrackerService.GetAsync<Store>(_newstoreId);
        if (store == null || string.IsNullOrEmpty(selectorTextBox) || string.IsNullOrEmpty(typeDataComboBox) || _newstoreId <= 0 || string.IsNullOrEmpty(getAttributeComboBox))
        {
            return false;
        }

        return true;
    }

    [RelayCommand]
    private async void SaveSelectors()
    {
        if (await ValidateSelectorData() == false) return;

        var store = await App.PriceTrackerService.GetAsync<Store>(_newstoreId);
        var newSelector = new Selector
        {
            CssSelector = selectorTextBox,
            Command = selectorTextBox,
            Type = typeDataComboBox,
            StoreId = _newstoreId,
            Store = store,
            RegexMethods = new List<RegexMethod>(),
            Attribute = getAttributeComboBox,
        };

        // Agregar los objetos RegexMethod a la lista RegexMethods
        foreach (var regexMethod in textBoxDataArray)
        {
            var newRegexMethod = new RegexMethod
            {
                Pattern = regexMethod.Pattern,
                Replacement = regexMethod.Replacement
            };
            newSelector.RegexMethods.Add(newRegexMethod);
        }

        await App.PriceTrackerService.InsertWithChildrenAsync<Selector>(newSelector, true);


        isRegistrationSuccessful = true;
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
        GetStore = await App.PriceTrackerService.GetWithChildrenAsync<Store>(_newstoreId);
        // Aquí puedes hacer algo con la variable _newstoreId, por ejemplo, asignarla a una propiedad del modelo de vista.
        var selectorsList = await App.PriceTrackerService.GetAllWithChildrenAsync<Selector>();
        var selectorsFromStore = selectorsList.Where(s => s.StoreId == _newstoreId).ToList();
        if (GetStore == null)
        {
            return;
        }
        foreach (var item in selectorsFromStore)
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
