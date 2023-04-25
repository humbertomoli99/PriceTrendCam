using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HtmlAgilityPack;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
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
    private int _newstoreId;

    [ObservableProperty]
    private string? typeDataComboBox;

    [ObservableProperty]
    private string? selectorTextBox;

    [ObservableProperty]
    private string? getAttributeComboBox;

    [ObservableProperty]
    private string? patternTextBox;

    [ObservableProperty]
    private string? replacementTextBox;

    [ObservableProperty]
    private bool isNotNullCheckBox;

    [ObservableProperty]
    private bool? hasFailures;

    public bool isRegistrationSuccessful;
    public bool isSelectorDeleted;

    public IWebViewService WebViewService
    {
        get;
    }

    [ObservableProperty]
    public Uri? source = new("https://pricetrendcam.blogspot.com/");

    [ObservableProperty]
    public bool isLoading = true;

    public string? SelectedCssSelector
    {
        get; set;
    }
    public List<RegexMethod>? TextBoxDataArray
    {
        get; set;
    }
    public WebView2? Webview
    {
        get;
        set;
    }
    public ObservableCollection<Selector> ListOfSelectors { get; set; } = new ObservableCollection<Selector>();
    public HtmlNode? HtmlDocumentStore
    {
        get;
        set;
    }
    public List<Selector>? CurrentUrlSelectors
    {
        get;
        set;
    }
    public Store? CurrentUrlStore
    {
        get;
        set;
    }
    //tab store data
    [ObservableProperty]
    private string? storeName;

    [ObservableProperty]
    private BitmapImage? faviconStore;

    [ObservableProperty]
    private string? webScrapingTool;

    [ObservableProperty]
    private string? htmlContent;

    public ObservableCollection<string>? StoreUrls { get; set; } = new ObservableCollection<string>();

    public AddSelectorsViewModel(IWebViewService webViewService)
    {
        WebViewService = webViewService;
        isRegistrationSuccessful = true;
        StoreUrls = new ObservableCollection<string>();
    }
    private async Task<List<StoreUrl>> GetStoreUrlsByUrl(string url)
    {
        var storesWithUrls = await App.PriceTrackerService.GetAllWithChildrenAsync<Store>();
        var matchingUrls = storesWithUrls
            .SelectMany(s => s.Urls)
            .Where(u => url.Contains(u.Url))
            .ToList();
        return matchingUrls;
    }
    private async Task<Store?> GetStoreDetailsAsync(string Url)
    {
        var urlShop = await GetStoreUrlsByUrl(Url);
        if (urlShop.Count > 0)
        {
            var partnerStore = await App.PriceTrackerService.GetWithChildrenAsync<Store>(urlShop.FirstOrDefault().StoreId);
            return partnerStore;
        }
        return null;
    }
    public void UpdateStoreDetails(Store partnerStore)
    {
        StoreName = partnerStore.Name;
        FaviconStore = new BitmapImage(new Uri(partnerStore.Favicon));
        WebScrapingTool = partnerStore.WebScrapingTool.ToString();

        StoreUrls.Clear();
        foreach (var url1 in partnerStore.Urls.ToList().Select(e => new StoreUrl { Url = e.Url }))
        {
            StoreUrls.Add(url1.Url);
        }
    }
    public async Task<List<Selector>> GetSelectorsFromStoreAsync()
    {
        if (CurrentUrlStore == null) return null;

        var selectorsList = await App.PriceTrackerService.GetAllWithChildrenAsync<Selector>();
        var selectorsFromStore = selectorsList.Where(s => s.StoreId == CurrentUrlStore.Id).ToList();

        return selectorsFromStore;
    }
    public async Task GetDataFromUrlAsync(string Url)
    {
        CurrentUrlStore = await GetStoreDetailsAsync(Url);

        if (CurrentUrlStore == null) return;

        CurrentUrlSelectors = await GetSelectorsFromStoreAsync();
        AddSelectorsToList(CurrentUrlSelectors);
        UpdateStoreDetails(CurrentUrlStore);
    }
    public void AddSelectorsToList(List<Selector> selectorsFromStore)
    {
        ListOfSelectors.Clear();

        foreach (var item in selectorsFromStore)
        {
            ListOfSelectors.Add(item);
        }
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
        if (store == null || string.IsNullOrEmpty(SelectorTextBox) || string.IsNullOrEmpty(TypeDataComboBox) || _newstoreId <= 0 || string.IsNullOrEmpty(GetAttributeComboBox))
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
            CssSelector = SelectorTextBox,
            Command = SelectorTextBox,
            Type = TypeDataComboBox,
            StoreId = _newstoreId,
            Store = store,
            RegexMethods = new List<RegexMethod>(),
            Attribute = GetAttributeComboBox,
            NotNull = IsNotNullCheckBox
        };

        // Agregar los objetos RegexMethod a la lista RegexMethods
        foreach (var regexMethod in TextBoxDataArray)
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
    private async Task<Store> GetStoreByIdAsync(int id)
    {
        return await App.PriceTrackerService.GetWithChildrenAsync<Store>(id);
    }
    public async Task OnNavigatedTo(object parameter)
    {
        if (parameter is not null and int)
        {
            _newstoreId = (int)parameter;
            CurrentUrlStore = await GetStoreByIdAsync(_newstoreId);
            var firstUrl = CurrentUrlStore.Urls.First().Url.ToString();

            await Webview.EnsureCoreWebView2Async(); // Asegura que la instancia de CoreWebView2 esté inicializada.

            Webview.CoreWebView2.Settings.IsScriptEnabled = true;

            if (CurrentUrlStore.WebScrapingTool == WebScrapingTools.HtmlAgilityPack)
            {
                Webview.CoreWebView2.Settings.IsBuiltInErrorPageEnabled = true;
                Webview.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                Webview.CoreWebView2.Settings.IsZoomControlEnabled = true;
                Webview.CoreWebView2.Settings.IsStatusBarEnabled = true;
                Webview.CoreWebView2.Settings.AreDevToolsEnabled = true;
                Webview.CoreWebView2.Settings.IsScriptEnabled = true;
                Webview.CoreWebView2.Settings.AreHostObjectsAllowed = true;
                Webview.CoreWebView2.Settings.IsWebMessageEnabled = true; // Habilita el modo sin conexión

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
        CurrentUrlStore = await App.PriceTrackerService.GetWithChildrenAsync<Store>(_newstoreId);
        if (CurrentUrlStore == null)
        {
            return;
        }
        // Aquí puedes hacer algo con la variable _newstoreId, por ejemplo, asignarla a una propiedad del modelo de vista.
        var selectorsList = await App.PriceTrackerService.GetAllWithChildrenAsync<Selector>();
        var selectorsFromStore = CurrentUrlStore.Selectors.Where(s => s.StoreId == _newstoreId).ToList();

        ListOfSelectors.Clear();
        foreach (var item in selectorsFromStore)
        {
            ListOfSelectors.Add(item);
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
            HasFailures = true;
        }
    }

    [RelayCommand]
    private void OnRetry()
    {
        HasFailures = false;
        IsLoading = true;
        WebViewService?.Reload();
    }
}
