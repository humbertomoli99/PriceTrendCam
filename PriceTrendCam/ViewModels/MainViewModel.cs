using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.Contracts.Services;
using PriceTrendCam.Core.Helpers;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Core.Services;
using PriceTrendCam.Views;
using Windows.ApplicationModel.DataTransfer;

namespace PriceTrendCam.ViewModels;
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string textBoxSearch;
    private bool SelectMultipleIsEnabled;
    private string message;
    private string content;
    private Visibility _isCheckedAllVisibility;
    private Visibility _deleteStoreVisibility;

    public Visibility isCheckedAllVisibility
    {
        get => _isCheckedAllVisibility;
        set => SetProperty(ref _isCheckedAllVisibility, value);
    }
    public Visibility DeleteStoreVisibility
    {
        get => _deleteStoreVisibility;
        set => SetProperty(ref _deleteStoreVisibility, value);
    }
    public XamlRoot XamlRoot
    {
        get;
        set;
    }

    public bool ClipboardAutomatically
    {
        get;
        set;
    }
    private ListView _ListView
    {
        get; set;
    }

    private readonly IClipboardSelectorService _clipboardSelectorService;
    public readonly ObservableCollection<ProductListItem> collection = new();
    public ICommand SelectMultiple => new RelayCommand(new Action(() => SelectMultipleCommand()));

    public MainViewModel(IClipboardSelectorService clipboardSelectorService)
    {
        _clipboardSelectorService = clipboardSelectorService;
        LoadProductsIntoList();
    }
    public MainViewModel(object[] campos)
    {
        _ListView = (ListView)campos[0];
        HideButtons();
    }
    private async void LoadProductsIntoList()
    {
        var products = await App.PriceTrackerService.GetAllWithChildrenAsync<ProductInfo>();
        InsertProductsIntoList(products);
    }
    public void SelectMultipleCommand()
    {
        var IsMultiSelect = _ListView.IsMultiSelectCheckBoxEnabled;
        var itemsSelected = _ListView.SelectedItems.Count;

        var AllItems = _ListView.Items.Count;
        if (AllItems > 0)
        {
            //_ListView.IsMultiSelectCheckBoxEnabled = true;
            if (SelectMultipleIsEnabled == false)
            {
                ShowButtons();
            }
            else if (SelectMultipleIsEnabled == true)
            {
                HideButtons();
            }
        }
    }
    public void InsertProductsIntoList(List<ProductInfo> Products)
    {
        collection.Clear();
        foreach (var item in Products)
        {
            var listProductsItem = new ProductListItem
            {
                Id = item.Id,
                Title = item.Name,
                ImageLocation = item.Image,
                Price = item.Price.ToString(),
                Stock = item.Stock == 0 ? "Stock Empty" : item.Stock == null ? "Not available" : item.Stock.ToString(),
                Shipping = item.ShippingPrice == 0 ? "Free shipping" : item.ShippingPrice == null ? "Not available" : item.ShippingPrice.ToString(),
            };

            collection.Add(listProductsItem);
        }
    }
    public async Task ShowMessageAddProductFromClipboard()
    {
        ClipboardAutomatically = await _clipboardSelectorService.LoadClipboardSettingFromSettingsAsync();

        if (!await IsUrlClipboardValid() && !ClipboardAutomatically) return;

        var urlClipboard = await GetClipboardTextAsync();
        if (await IsRegistered(urlClipboard)) return;

        var urlShop = await GetStoreUrlsByUrl(urlClipboard);
        if (urlShop.Count == 0) return;

        //mensaje de si desea añadir el producto en contrado
        await SearchUrlAsync(await GetClipboardTextAsync());
    }

    public async Task<bool> IsUrlClipboardValid()
    {
        var urlClipboard = await GetClipboardTextAsync();

        if (string.IsNullOrWhiteSpace(urlClipboard))
        {
            return await Url.IsValid(urlClipboard);
        }
        return false;
    }

    public async Task<string> GetClipboardTextAsync()
    {
        var package = Clipboard.GetContent();
        if (package.Contains(StandardDataFormats.Text))
        {
            var text = await package.GetTextAsync();
            return text;
        }
        return string.Empty;
    }

    [RelayCommand]
    public async Task AdvancedSearch()
    {
        try
        {
            if (await Url.IsValid(textBoxSearch))
            {
                await SearchUrlAsync(textBoxSearch);
            }
            else
            {
                await SearchTermAsync();
            }
        }
        catch (Exception ex)
        {
            // Manejo de excepciones
            var dialog = new ContentDialog
            {
                Title = "Error",
                XamlRoot = XamlRoot,
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Close,
                Content = "An error occurred: " + ex.Message
            };

            await dialog.ShowAsync();
        }
    }

    private async Task<bool> IsRegistered(string url)
    {
        // Buscar si la URL tiene un sitemap y selectores asignados
        var productList = await App.PriceTrackerService.GetAllWithChildrenAsync<ProductInfo>();
        var isRegistered = ((productList?.Where(s => s?.Url?.Equals(url) ?? false)?.ToList().Count ?? 0) > 0);
        return isRegistered;
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
    private async Task SearchUrlAsync(string url)
    {
        if (await IsRegistered(url))
        {
            message = "The product is registered";
            content = "The product is already registered and will continue to be tracked, don't worry";
            await ShowMessageError();
            return;
        }

        var urlShop = await GetStoreUrlsByUrl(url);

        if (urlShop.Count == 0)
        {
            message = "The url is not registered in Stores";
            content = "The url is not registered in Stores, please assign selectors and add start url";
            await ShowMessageError();
            return;
        }

        var partnerStore = await App.PriceTrackerService.GetWithChildrenAsync<Store>(urlShop.FirstOrDefault().StoreId);

        if (partnerStore.Urls == null || partnerStore.Urls.Count == 0)
        {
            message = "No selectors assigned to Store";
            content = "The url does not have selectors assigned, we recommend you see the tutorial on how to add one";
            await ShowMessageError();
            return;
        }

        var selectorsList = await App.PriceTrackerService.GetAllWithChildrenAsync<Selector>();
        var selectorsFromStore = selectorsList.Where(s => s.StoreId == partnerStore.Id).ToList();

        var newProduct = await ParseInfoService.GetProductFromUrl(url, partnerStore, selectorsFromStore);

        var isSucces = await InsertProduct(newProduct);

        if (isSucces == true)
        {
            message = "Product Inserted";
            content = newProduct.Name + "\n" + newProduct.Price + "\n" + newProduct.Stock;
        }
        else
        {
            message = "Not Inserted";
            content = "The product has not add";
        }

        await ShowMessageError();
    }
    private async Task<bool> InsertProduct(ProductInfo newProduct)
    {
        try
        {
            return await App.PriceTrackerService.InsertAsync(newProduct) > 0;
        }
        catch (Exception ex)
        {
            // Manejar la excepción (log, mostrar mensaje, etc.)
            return false;
        }
    }

    private async Task SearchTermAsync()
    {
        // Utilizar los diccionarios ParseWebSiteJsonLdForSearchAction para obtener una URL de búsqueda y obtener el buscador de la web
        // Si no hay ningún sitemap, informar al usuario de cómo realizar este proceso
        var productList = await App.PriceTrackerService.GetAllAsync<ProductInfo>();
        // Si no hay buscadores, solo decir al usuario los productos que tiene en seguimiento
    }

    private async Task ShowMessageError()
    {
        // El producto ha sido agregado
        var dialog = new ContentDialog
        {
            Title = message,
            XamlRoot = XamlRoot,
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Close,
            Content = content
        };

        await dialog.ShowAsync();
    }
    private void HideButtons()
    {
        _ListView.SelectedItem = null;
        SelectMultipleIsEnabled = false;
        _ListView.SelectionMode = ListViewSelectionMode.Single;
        isCheckedAllVisibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        DeleteStoreVisibility = Microsoft.UI.Xaml.Visibility.Collapsed;
    }
    private void ShowButtons()
    {
        SelectMultipleIsEnabled = true;
        _ListView.SelectionMode = ListViewSelectionMode.Multiple;
        isCheckedAllVisibility = Microsoft.UI.Xaml.Visibility.Visible;
        DeleteStoreVisibility = Microsoft.UI.Xaml.Visibility.Visible;
    }
}
