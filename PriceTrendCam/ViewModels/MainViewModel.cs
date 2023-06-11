using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using PriceTrendCam.Contracts.Services;
using PriceTrendCam.Core.Helpers;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Core.Services;
using PriceTrendCam.Helpers;
using PriceTrendCam.Models;
using PriceTrendCam.Views;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Metadata;

namespace PriceTrendCam.ViewModels;
public partial class MainViewModel : MainModel
{
    public async Task OnSelectedItemChanged()
    {
        if (UnsortedProducts == null) return;
        var isAscending = (previousSelectedSortDirection == "Ascending");
        var sortedProducts = await GetOrderedList(UnsortedProducts, OrderBy, isAscending);

        var pagedItems = await GetPagedItems(sortedProducts, CurrentPageIndex, SelectedRowsPerPageOption);

        InsertProductsIntoList(pagedItems);

        await UpdatePageCommands();
    }

    public MainViewModel(INavigationService navigationService, IClipboardSelectorService clipboardSelectorService = null)
    {
        _clipboardSelectorService = clipboardSelectorService;
        _navigationService = navigationService;

        ContentDialogHelper = ContentDialogHelper<ContentDialog>.Instance;

        previousSelectedSortBy = "Id";
        previousSelectedSortDirection = "Descending";

        ListViewCollection = new ObservableCollection<ProductListItem>();
        _ = HideButtons();

        SelectedRowsPerPageOption = 5;

        RowsPerPageOptions = new ObservableCollection<int> { 5, 10, 25, 50, 100 };
        UnsortedProducts = new List<ProductInfo>();

        _ = UpdateList();
    }

    public async Task<List<ProductInfo>> GetUnsortedProducts()
    {
        List<ProductInfo> unsortedProducts = await App.PriceTrackerService.GetAllWithChildrenAsync<ProductInfo>();
        return unsortedProducts;
    }

    public void Pagination(int totalItemsCount, int defaultRowsPerPage = 10)
    {
        TotalItemsCount = totalItemsCount;
        SelectedRowsPerPageOption = defaultRowsPerPage;
    }

    [RelayCommand(CanExecute = nameof(CanMoveToFirstPage))]
    private async Task MoveToFirstPage()
    {
        CurrentPageIndex = 0;
        await UpdatePageCommands();
    }

    [RelayCommand(CanExecute = nameof(CanMoveToPreviousPage))]
    private async Task MoveToPreviousPage()
    {
        CurrentPageIndex--;
        await UpdatePageCommands();
    }

    [RelayCommand(CanExecute = nameof(CanMoveToNextPage))]
    private async Task MoveToNextPage()
    {
        CurrentPageIndex++;
        await UpdatePageCommands();
    }

    [RelayCommand(CanExecute = nameof(CanMoveToLastPage))]
    private async Task MoveToLastPage()
    {
        CurrentPageIndex = TotalPagesCount - 1;
        await UpdatePageCommands();
    }


    private bool CanMoveToFirstPage() => CurrentPageIndex > 0;
    private bool CanMoveToPreviousPage() => CurrentPageIndex > 0;
    private bool CanMoveToNextPage() => CurrentPageIndex < TotalPagesCount - 1;
    private bool CanMoveToLastPage() => CurrentPageIndex != TotalPagesCount - 1;

    private async Task UpdatePageCommands()
    {
        MoveToPreviousPageCommand.NotifyCanExecuteChanged();
        MoveToNextPageCommand.NotifyCanExecuteChanged();
        MoveToLastPageCommand.NotifyCanExecuteChanged();
        MoveToFirstPageCommand.NotifyCanExecuteChanged();

        await UpdateList();

        var isAscending = (previousSelectedSortDirection == "Ascending");
        var sortedProducts = await GetOrderedList(UnsortedProducts, OrderBy, isAscending);

        var pagedItems  = await GetPagedItems(sortedProducts, CurrentPageIndex, SelectedRowsPerPageOption);

        InsertProductsIntoList(pagedItems);
    }
    [RelayCommand]
    private async Task DeleteProduct()
    {
        try
        {
            var itemsSelected = ListViewProductsControl.SelectedItems;
            if (itemsSelected.Count > 0)
            {
                string content = GetDeleteDialogContent(itemsSelected.Count);

                var result = await ShowDeleteConfirmationDialog(content);

                if (result == ContentDialogResult.Primary)
                {
                    await DeleteSelectedProducts(itemsSelected);
                    await UpdateList();
                    await HideButtons();
                }
                else if (result == ContentDialogResult.None)
                {
                    await HideButtons();
                }
            }
        }
        catch (Exception ex)
        {
            // Manejar la excepción de acuerdo a tus necesidades
            Console.WriteLine($"Error al eliminar el producto: {ex.Message}");
        }
    }

    private string GetDeleteDialogContent(int itemsCount)
    {
        if (itemsCount == 1)
        {
            return "¿Está seguro de eliminar el registro?\nSe dejarán de seguir los productos relacionados con la tienda.";
        }
        else
        {
            return $"¿Está seguro de eliminar los {itemsCount} registros?\nSe dejarán de seguir los productos relacionados con las tiendas.";
        }
    }

    private async Task<ContentDialogResult> ShowDeleteConfirmationDialog(string content)
    {
        var deleteFileDialog = new ContentDialog
        {
            Title = "Delete Product",
            XamlRoot = XamlRoot,
            Content = content,
            DefaultButton = ContentDialogButton.Primary,
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel"
        };

        return await ContentDialogHelper<ContentDialog>.Instance.ShowContentDialog(deleteFileDialog);
    }

    private async Task DeleteSelectedProducts(IList<object> itemsSelected)
    {
        foreach (var item in itemsSelected)
        {
            var data = (ProductListItem)item;
            await App.PriceTrackerService.DeleteAsync<ProductInfo>(data.Id);
        }
    }

    [RelayCommand]
    public async Task UpdateList()
    {
        UnsortedProducts = await GetUnsortedProducts();
        var isAscending = (previousSelectedSortDirection == "Ascending");
        TotalItemsCount = UnsortedProducts.Count;

        var sortedProducts = await GetOrderedList(UnsortedProducts, previousSelectedSortBy, isAscending);

        var pagedItems = await GetPagedItems(sortedProducts, CurrentPageIndex, SelectedRowsPerPageOption);

        InsertProductsIntoList(pagedItems);
    }

    [RelayCommand]
    private async Task FilterList()
    {
        await HideButtons();
    }

    [RelayCommand]
    private async Task OrderList()
    {
        var dialogOrderList = new OrderListContentDialog(previousSelectedSortBy, previousSelectedSortDirection)
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style
        };

        var dialogResult = await ContentDialogHelper.ShowContentDialog(dialogOrderList);

        if (dialogResult != ContentDialogResult.Primary)
            return;

        var sortByPanel = dialogOrderList.FindName("SortByPanel") as StackPanel;
        var selectedSortBy = GetSelectedRadioButton(sortByPanel).Tag.ToString();

        var sortDirectionPanel = dialogOrderList.FindName("SortDirectionPanel") as StackPanel;
        var selectedSortDirection = GetSelectedRadioButton(sortDirectionPanel).Tag.ToString();

        var isAscending = (selectedSortDirection == "Ascending");

        var sortedProducts = await GetOrderedList(UnsortedProducts, selectedSortBy, isAscending);

        var pagedItems = await GetPagedItems(sortedProducts, CurrentPageIndex, SelectedRowsPerPageOption);

        InsertProductsIntoList(pagedItems);

        previousSelectedSortBy = selectedSortBy;
        previousSelectedSortDirection = selectedSortDirection;
    }

    private RadioButton GetSelectedRadioButton(StackPanel stackPanel)
    {
        foreach (var child in stackPanel.Children)
        {
            if (child is RadioButton radioButton && radioButton.IsChecked == true)
            {
                return radioButton;
            }
        }
        return null;
    }
    public async Task<List<ProductInfo>> GetOrderedList(List<ProductInfo> unsortedProducts, string property = "Id", bool ascendant = false)
    {
        try
        {
            var sortedProducts = OrderListByProperty(unsortedProducts, property, ascendant);
            return sortedProducts;
        }
        catch (Exception ex)
        {
            await ContentDialogHelper.ShowExceptionDialog(ex, XamlRoot);
            return null;
        }
    }
    public async Task<List<ProductInfo>> GetPagedItems(List<ProductInfo> orderedProducts, int page = 0, int pageSize = 10)
    {
        try
        {
            var itemsOnPage = orderedProducts.Skip(page * pageSize).Take(pageSize).ToList();
            TotalPagesCount = CalculateTotalPages(orderedProducts.Count, pageSize);
            CurrentPageIndex = page;

            OnPropertyChanged(nameof(PageSummary));
            OnPropertyChanged(nameof(TotalItemsCount));

            return itemsOnPage;
        }
        catch (Exception ex)
        {
            await ContentDialogHelper.ShowExceptionDialog(ex, XamlRoot);
            return null;
        }
    }
    private List<ProductInfo> OrderListByProperty(List<ProductInfo> unsortedProducts, string property, bool ascendant)
    {
        if (property == null)
        {
            return unsortedProducts; // Retorna la lista desordenada si el parámetro property es null
        }

        var propertySelectors = new Dictionary<string, Func<ProductInfo, object>>
        {
            { "Status", p => p.Status },
            { "Name", p => p.Name },
            { "Id", p => p.Id },
            { "Price", p => p.Price },
            { "ShippingPrice", p => p.ShippingPrice },
            { "Stock", p => p.Stock }
        };

        if (propertySelectors.ContainsKey(property))
        {
            return ascendant ? unsortedProducts.OrderBy(propertySelectors[property]).ToList() : unsortedProducts.OrderByDescending(propertySelectors[property]).ToList();
        }

        return unsortedProducts; // Retorna la lista desordenada si el parámetro property no coincide con ninguna propiedad válida
    }

    private int CalculateTotalPages(int totalItems, int pageSize)
    {
        // Implementa la lógica para calcular el número total de páginas en función del tamaño de página
        // Puedes usar matemáticas básicas para realizar el cálculo, por ejemplo:
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        return totalPages;
    }

    private List<ProductInfo> GetItemsForPage(int page, int pageSize)
    {
        return UnsortedProducts.Skip(page * pageSize).Take(pageSize).ToList();
    }

    [RelayCommand]
    private async void SelectMultiple()
    {
        if (ListViewProductsControl == null || ListViewProductsControl.Items.Count == 0)
        {
            return;
        }

        if (SelectMultipleIsEnabled)
        {
            await HideButtons();
        }
        else
        {
            await ShowButtons();
        }
    }

    private async void DeleteCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (args.Parameter != null)
        {
            foreach (var i in ListViewCollection)
            {
                if (i.Id == (args.Parameter as int?))
                {
                    ListViewCollection.Remove(i);
                    _ = await App.PriceTrackerService.DeleteAsync<ProductInfo>(i.Id);
                    await UpdateList();
                    return;
                }
            }
        }
        if (ListViewProductsControl.SelectedIndex != -1)
        {
            ListViewCollection.RemoveAt(ListViewProductsControl.SelectedIndex);
        }
    }
    public async void InsertProductsIntoList(List<ProductInfo> Products)
    {
        try
        {
            ListViewCollection.Clear();
            var deleteCommand = new StandardUICommand(StandardUICommandKind.Delete);
            deleteCommand.ExecuteRequested += DeleteCommand_ExecuteRequested;

            foreach (var item in Products)
            {
                var listItemData = new ProductListItem
                {
                    Id = item.Id,
                    Title = item.Name,
                    ImageLocation = string.IsNullOrEmpty(item.Image) ? "ms-appx:///Assets/Mountain_Monochromatic.png" : item.Image,
                    Price = item.Price.ToString(),
                    Stock = item.Stock == 0 ? "Stock Empty" : (item.Stock == null ? "Not available" : item.Stock.ToString()),
                    Shipping = item.ShippingPrice == 0 ? "Free shipping" : (item.ShippingPrice == null ? "Not available" : item.ShippingPrice.ToString()),
                    Command = (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7) ? deleteCommand : null)
                };

                ListViewCollection.Add(listItemData);
            }

            OnPropertyChanged(nameof(PageSummary));
            OnPropertyChanged(nameof(TotalItemsCount));

            Pagination(TotalItemsCount, SelectedRowsPerPageOption);
        }
        catch (Exception ex)
        {
            await ContentDialogHelper.ShowExceptionDialog(ex, XamlRoot);
        }
    }

    public async Task ShowMessageAddProductFromClipboard()
    {
        // Cargar la configuración del portapapeles automáticamente
        ClipboardAutomatically = await _clipboardSelectorService.LoadClipboardSettingFromSettingsAsync();

        // Comprobar si el portapapeles es válido o si está habilitada la opción de portapapeles automático
        if (await IsUrlClipboardValid() || ClipboardAutomatically)
            return; // No hacer nada si no se cumple la condición

        // Obtener el contenido del portapapeles
        var urlClipboard = await GetClipboardTextAsync();

        // Comprobar si la URL del portapapeles ya está registrada
        if (await IsRegistered(urlClipboard))
            return; // No hacer nada si ya está registrada

        // Obtener las URL de la tienda correspondientes a la URL del portapapeles
        var urlShop = await GetStoreUrlsByUrl(urlClipboard);

        // Comprobar si se encontraron URLs de tiendas y si no es nulo
        if (urlShop == null || urlShop.Count == 0)
            return; // No hacer nada si no se encontraron URLs de tiendas o si es nulo

        // Realizar la búsqueda de la URL del portapapeles
        await SearchUrlAsync(urlClipboard);
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
    public async Task SearchInListView()
    {
        var UnsortedProducts = await GetUnsortedProducts();
        if (TextBoxSearchListView == null) return;

        var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
        UnsortedProducts = UnsortedProducts.Where(p => compareInfo.IndexOf(p.Name, TextBoxSearchListView, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) >= 0).ToList();

        var isAscending = (previousSelectedSortDirection == "Ascending");
        var sortedProducts = await GetOrderedList(UnsortedProducts, previousSelectedSortBy, isAscending);

        var pagedItems = await GetPagedItems(sortedProducts, CurrentPageIndex, SelectedRowsPerPageOption);

        InsertProductsIntoList(pagedItems);

        OnPropertyChanged(nameof(PageSummary));
        OnPropertyChanged(nameof(TotalItemsCount));
    }
    [RelayCommand]
    public async Task AdvancedSearch()
    {
        try
        {
            Uri createdUri;

            var success = Url.TryCreateUri(TextBoxSearch, out createdUri);

            if (success)
            {
                // El Uri se creó exitosamente
                Console.WriteLine("Uri creado: " + createdUri.ToString());
                await SearchUrlAsync(TextBoxSearch);
            }
            else
            {
                // La creación del Uri falló
                Console.WriteLine("No se pudo crear el Uri.");
                await SearchTermAsync();
            }
        }
        catch (Exception ex)
        {
            await ContentDialogHelper.ShowExceptionDialog(ex, XamlRoot);
        }
    }

    private async Task<bool> IsRegistered(string url)
    {
        try
        {
            // Buscar si la URL tiene un sitemap y selectores asignados
            var productList = await GetUnsortedProducts();
            var isRegistered = ((productList?.Where(s => s?.Url?.Equals(url) ?? false)?.ToList().Count ?? 0) > 0);
            return isRegistered;
        }
        catch (Exception ex)
        {
            await ContentDialogHelper.ShowExceptionDialog(ex, XamlRoot);
            return false;
        }

    }
    private async Task<List<StoreUrl>?> GetStoreUrlsByUrl(string url)
    {
        try
        {
            var storesWithUrls = await App.PriceTrackerService.GetAllWithChildrenAsync<Store>();
            var matchingUrls = storesWithUrls
                .SelectMany(s => s.Urls)
                .Where(u => url.Contains(u.Url))
                .ToList();
            return matchingUrls;
        }
        catch (Exception ex)
        {
            await ContentDialogHelper.ShowExceptionDialog(ex, XamlRoot);
            return null;
        }
    }
    private async Task SearchUrlAsync(string url)
    {
        try
        {
            if (await IsRegistered(url))
            {
                message = "The product is registered";
                content = "The product is already registered and will continue to be tracked, don't worry";
                await ShowMessageError();
                return;
            }

            var urlShop = await GetStoreUrlsByUrl(url);

            if (urlShop.Count == 0 || urlShop == null)
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
                await UpdateList();
            }
            else
            {
                message = "Not Inserted";
                content = "The product has not add";
            }

            await ShowMessageError();
        }
        catch (Exception ex)
        {
            await ContentDialogHelper.ShowExceptionDialog(ex, XamlRoot);
        }
    }
    private async Task<bool> InsertProduct(ProductInfo newProduct)
    {
        try
        {
            await App.PriceTrackerService.InsertAsync(newProduct);

            var firstHistory = new History()
            {
                ProductInfoId = newProduct.Id,
                ProductInfo = newProduct,
                Price = newProduct.Price,
                ShippingPrice = newProduct.ShippingPrice,
                Stock = newProduct.Stock,
                Date = newProduct.Date,
            };
            await App.PriceTrackerService.InsertAsync<History>(firstHistory);

            return true;
        }
        catch (Exception ex)
        {
            await ContentDialogHelper.ShowExceptionDialog(ex, XamlRoot);
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

        await ContentDialogHelper<ContentDialog>.Instance.ShowContentDialog(dialog);
    }
    private async Task HideButtons()
    {
        try
        {
            if (ListViewProductsControl != null)
            {
                ListViewProductsControl.SelectedItem = null;
                ListViewProductsControl.SelectionMode = ListViewSelectionMode.Single;
            }
            SelectMultipleIsEnabled = false;
            IsCheckedAllVisibility = Visibility.Collapsed;
            DeleteProductVisibility = Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            await ContentDialogHelper.ShowExceptionDialog(ex, XamlRoot);
        }
    }
    private async Task ShowButtons()
    {
        try
        {
            if (ListViewProductsControl != null)
            {
                ListViewProductsControl.SelectionMode = ListViewSelectionMode.Multiple;
            }
            SelectMultipleIsEnabled = true;
            IsCheckedAllVisibility = Visibility.Visible;
            DeleteProductVisibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            await ContentDialogHelper.ShowExceptionDialog(ex, XamlRoot);
        }
    }

    public async Task HandleSelectionChangedAsync(IList<object> selectedItems)
    {
        try
        {
            var itemsSelected = selectedItems.Count;
            var AllItems = ListViewProductsControl.Items.Count;
            if (ListViewProductsControl.SelectionMode == ListViewSelectionMode.Multiple || ListViewProductsControl.SelectionMode == ListViewSelectionMode.Extended)
            {
                if (itemsSelected == AllItems)
                {
                    SelectAllCheckBox.IsChecked = true;
                    SelectAllCheckBoxIcon.Glyph = "\ue73a";
                }
                else if (itemsSelected == 0)
                {
                    SelectAllCheckBox.IsChecked = false;
                    SelectAllCheckBoxIcon.Glyph = "\ue739";
                }
                else
                {
                    SelectAllCheckBox.IsChecked = false;
                    SelectAllCheckBoxIcon.Glyph = "\uf16e";
                }
            }
            if (ListViewProductsControl.SelectionMode == ListViewSelectionMode.Single && selectedItems.Count > 0)
            {
                var obj = (ProductListItem)ListViewProductsControl.SelectedItem;
                _navigationService.NavigateTo(typeof(ProductDetailsViewModel).FullName!, obj.Id);
            }
        }
        catch (Exception ex)
        {
            await ContentDialogHelper.ShowExceptionDialog(ex, XamlRoot);
        }
    }
}
