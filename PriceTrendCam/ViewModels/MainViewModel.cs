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
    private Visibility _deleteProductVisibility;

    public Visibility IsCheckedAllVisibility
    {
        get => _isCheckedAllVisibility;
        set => SetProperty(ref _isCheckedAllVisibility, value);
    }
    public Visibility DeleteProductVisibility
    {
        get => _deleteProductVisibility;
        set => SetProperty(ref _deleteProductVisibility, value);
    }
    public XamlRoot xamlRoot
    {
        get;
        set;
    }

    public bool ClipboardAutomatically
    {
        get;
        set;
    }

    private readonly IClipboardSelectorService _clipboardSelectorService;
    public ICommand UpdateList => new RelayCommand(async () => await UpdateListCommand());
    public ICommand DeleteProduct => new RelayCommand<object>(async (parameter) => await DeleteProductCommand(parameter));

    private ObservableCollection<ProductListItem> _listViewCollection;
    public ObservableCollection<ProductListItem> ListViewCollection
    {
        get => _listViewCollection;
        set => SetProperty(ref _listViewCollection, value);
    }
    public ListView ListViewProducts
    {
        get;
        set;
    }

    private async Task DeleteProductCommand(object parameter)
    {
        try
        {
            var xamlRoot2 = parameter as Microsoft.UI.Xaml.XamlRoot;
            IList<object> itemsSelected = ListViewProducts.SelectedItems;
            if (itemsSelected.Count > 0)
            {
                var itemsS = itemsSelected.Count.ToString();
                string content;
                if (itemsSelected.Count == 1)
                {
                    content = $"Esta seguro de eliminar el registro?\nSe dejaran de seguir los productos relacionados con la tienda";
                }
                else
                {
                    content = $"Esta seguro de eliminar los {itemsS} registros?\nSe dejaran de seguir los productos relacionados con las tiendas";
                }

                ContentDialog deleteFileDialog = new ContentDialog
                {
                    Title = "Delete Product",
                    XamlRoot = xamlRoot2,
                    Content = content,
                    DefaultButton = ContentDialogButton.Primary,
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel"
                };

                ContentDialogResult result = await deleteFileDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    foreach (var item in itemsSelected)
                    {
                        ProductListItem data = (ProductListItem)item;
                        await App.PriceTrackerService.DeleteAsync<ProductInfo>(data.Id);
                    }
                    await LoadProductsAsync();
                    HideButtons();
                }
                else if (result == ContentDialogResult.None)
                {
                    HideButtons();
                }
            }
        }
        catch (Exception ex)
        {
            var dialog = new ContentDialog
            {
                Title = ex.ToString(),
                XamlRoot = xamlRoot,
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Close,
                Content = ex.Message
            };

            await dialog.ShowAsync();
        }
    }

    private async Task UpdateListCommand()
    {
        await LoadProductsAsync();
    }

    public MainViewModel(IClipboardSelectorService clipboardSelectorService = null)
    {
        _clipboardSelectorService = clipboardSelectorService;

        ListViewCollection = new ObservableCollection<ProductListItem>();
        HideButtons();
    }
    [RelayCommand]
    private void FilterList()
    {
        HideButtons();
    }
    [RelayCommand]
    private void OrderList()
    {
        ShowButtons();
    }
    public async Task LoadProductsAsync()
    {
        // Obtener los productos de alguna fuente de datos
        List<ProductInfo> products = await App.PriceTrackerService.GetAllWithChildrenAsync<ProductInfo>();

        // Insertar los productos en la lista
        InsertProductsIntoList(products);
    }
    [RelayCommand]
    private void SelectMultiple()
    {
        if (ListViewProducts == null) return;

        var AllItems = ListViewProducts.Items.Count;
        if (AllItems > 0)
        {
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
        ListViewCollection.Clear();
        foreach (var item in Products)
        {
            var listProductsItem = new ProductListItem()
            {
                Id = item.Id,
                Title = item.Name,
                ImageLocation = item.Image,
                Price = item.Price.ToString(),
                Stock = item.Stock == 0 ? "Stock Empty" : item.Stock == null ? "Not available" : item.Stock.ToString(),
                Shipping = item.ShippingPrice == 0 ? "Free shipping" : item.ShippingPrice == null ? "Not available" : item.ShippingPrice.ToString(),
            };
            ListViewCollection.Add(listProductsItem);
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
                XamlRoot = xamlRoot,
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
            await LoadProductsAsync();
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
            XamlRoot = xamlRoot,
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Close,
            Content = content
        };

        await dialog.ShowAsync();
    }
    private void HideButtons()
    {
        if (ListViewProducts != null)
        {
            ListViewProducts.SelectedItem = null;
            ListViewProducts.SelectionMode = ListViewSelectionMode.Single;
        }
        SelectMultipleIsEnabled = false;
        IsCheckedAllVisibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        DeleteProductVisibility = Microsoft.UI.Xaml.Visibility.Collapsed;
    }
    private void ShowButtons()
    {
        if (ListViewProducts != null)
        {
            ListViewProducts.SelectionMode = ListViewSelectionMode.Multiple;
        }
        SelectMultipleIsEnabled = true;
        IsCheckedAllVisibility = Microsoft.UI.Xaml.Visibility.Visible;
        DeleteProductVisibility = Microsoft.UI.Xaml.Visibility.Visible;
    }
}
