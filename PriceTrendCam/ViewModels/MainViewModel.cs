using System.Collections.ObjectModel;
using System.Globalization;
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
using PriceTrendCam.Views;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Metadata;

namespace PriceTrendCam.ViewModels;
public partial class MainViewModel : ObservableObject
{
    public ContentDialogHelper<ContentDialog> ContentDialogHelper
    {
        get; set;
    }
    public XamlRoot XamlRoot
    {
        get; set;
    }
    private int _totalItemsCount;
    private int _currentPageIndex;
    private int _rowsPerPage;
    public int TotalItemsCount => ListViewCollection.Count;
    public int TotalPagesCount;
    public string PageSummary => $"Página {_currentPageIndex + 1} de {TotalPagesCount}";
    public ObservableCollection<int> RowsPerPageOptions
    {
        get; set;
    }
    private int _selectedRowsPerPageOption;
    public int SelectedRowsPerPageOption
    {
        get => _selectedRowsPerPageOption;
        set
        {
            if (SetProperty(ref _selectedRowsPerPageOption, value))
            {
                _ = OnSelectedItemChanged();
            }
        }
    }

    private async Task OnSelectedItemChanged()
    {
        if (ProductsList == null) return;
        var isAscending = (previousSelectedSortDirection == "Ascending");
        await GetOrderedList(OrderBy, isAscending, CurrentPageIndex, SelectedRowsPerPageOption);
        await UpdatePageCommands();
    }

    public int CurrentPageIndex
    {
        get => _currentPageIndex;
        set => SetProperty(ref _currentPageIndex, value);
    }
    [ObservableProperty]
    private string textBoxSearch;

    [ObservableProperty]
    private string textBoxSearchListView;

    private bool SelectMultipleIsEnabled;
    private string message;
    private string content;
    public bool OrderDescen;

    [ObservableProperty]
    public Visibility isCheckedAllVisibility;

    [ObservableProperty]
    public Visibility deleteProductVisibility;

    public bool ClipboardAutomatically
    {
        get;
        set;
    }

    private readonly IClipboardSelectorService _clipboardSelectorService;
    private readonly INavigationService _navigationService;

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
    public AppBarToggleButton SelectAllCheckBox
    {
        get;
        set;
    }
    public FontIcon SelectAllCheckBoxIcon
    {
        get;
        set;
    }
    public string OrderBy
    {
        get;
        private set;
    }
    public List<ProductInfo> ProductsList
    {
        get;
        private set;
    }

    private string previousSelectedSortBy;
    private string previousSelectedSortDirection;

    public IAsyncRelayCommand MoveToFirstPageCommand
    {
        get;
    }
    public IAsyncRelayCommand MoveToPreviousPageCommand
    {
        get;
    }
    public IAsyncRelayCommand MoveToNextPageCommand
    {
        get;
    }
    public IAsyncRelayCommand MoveToLastPageCommand
    {
        get;
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

        MoveToFirstPageCommand = new AsyncRelayCommand(MoveToFirstPage,CanMoveToFirstPage);
        MoveToPreviousPageCommand = new AsyncRelayCommand(MoveToPreviousPage, CanMoveToPreviousPage);
        MoveToNextPageCommand = new AsyncRelayCommand(MoveToNextPage, CanMoveToNextPage);
        MoveToLastPageCommand = new AsyncRelayCommand(MoveToLastPage, CanMoveToLastPage);

        SelectedRowsPerPageOption = 10;
    }

    public void Pagination(int totalItemsCount, int defaultRowsPerPage = 10)
    {
        _totalItemsCount = totalItemsCount;
        _rowsPerPage = defaultRowsPerPage;

        RowsPerPageOptions = new ObservableCollection<int> { 10, 25, 50, 100 };
        SelectedRowsPerPageOption = defaultRowsPerPage;
    }

    private async Task MoveToFirstPage()
    {
        CurrentPageIndex = 0;
        await UpdatePageCommands();
    }

    private async Task MoveToPreviousPage()
    {
        CurrentPageIndex--;
        await UpdatePageCommands();
    }

    private async Task MoveToNextPage()
    {
        CurrentPageIndex++;
        await UpdatePageCommands();
    }

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

        var isAscending = (previousSelectedSortDirection == "Ascending");
        await GetOrderedList(OrderBy, isAscending, CurrentPageIndex, SelectedRowsPerPageOption);
    }

    [RelayCommand]
    private async Task DeleteProduct()
    {
        try
        {
            var itemsSelected = ListViewProducts.SelectedItems;
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

                var deleteFileDialog = new ContentDialog
                {
                    Title = "Delete Product",
                    XamlRoot = XamlRoot,
                    Content = content,
                    DefaultButton = ContentDialogButton.Primary,
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel"
                };

                var result = await ContentDialogHelper<ContentDialog>.Instance.ShowContentDialog(deleteFileDialog);

                if (result == ContentDialogResult.Primary)
                {
                    foreach (var item in itemsSelected)
                    {
                        var data = (ProductListItem)item;
                        await App.PriceTrackerService.DeleteAsync<ProductInfo>(data.Id);
                    }
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
            await ContentDialogHelper.ShowExceptionDialog(ex, XamlRoot);
        }
    }
    [RelayCommand]
    public async Task UpdateList()
    {
        ProductsList = await App.PriceTrackerService.GetAllWithChildrenAsync<ProductInfo>();
        var isAscending = (previousSelectedSortDirection == "Ascending");

        await GetOrderedList(previousSelectedSortBy, isAscending, CurrentPageIndex, SelectedRowsPerPageOption);
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
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            SelectedSortBy = previousSelectedSortBy, // establecer valores previos
            SelectedSortDirection = previousSelectedSortDirection
        };

        var dialogResult = await ContentDialogHelper.ShowContentDialog(dialogOrderList);

        if (dialogResult != ContentDialogResult.Primary)
            return;

        var sortByPanel = dialogOrderList.FindName("SortByPanel") as StackPanel;
        var selectedSortBy = GetSelectedRadioButton(sortByPanel).Tag.ToString();

        var sortDirectionPanel = dialogOrderList.FindName("SortDirectionPanel") as StackPanel;
        var selectedSortDirection = GetSelectedRadioButton(sortDirectionPanel).Tag.ToString();

        var isAscending = (selectedSortDirection == "Ascending");

        await GetOrderedList(selectedSortBy, isAscending, CurrentPageIndex, SelectedRowsPerPageOption);

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
    public async Task GetOrderedList(string order = "id", bool Ascendant = false, int page = 0, int pageSize = 10)
    {
        try
        {
            // Ordenar la lista de productos en función de la columna de ordenamiento y el orden ascendente/descendente
            switch (order)
            {
                case "Status":
                    ProductsList = Ascendant ? ProductsList.OrderBy(o => o.Status).ToList() : ProductsList.OrderByDescending(o => o.Status).ToList();
                    break;
                case "Name":
                    ProductsList = Ascendant ? ProductsList.OrderBy(o => o.Name).ToList() : ProductsList.OrderByDescending(o => o.Name).ToList();
                    break;
                case "Id":
                    ProductsList = Ascendant ? ProductsList.OrderBy(o => o.Id).ToList() : ProductsList.OrderByDescending(o => o.Id).ToList();
                    break;
                case "Price":
                    ProductsList = Ascendant ? ProductsList.OrderBy(o => o.Price).ToList() : ProductsList.OrderByDescending(o => o.Price).ToList();
                    break;
                case "ShippingPrice":
                    ProductsList = Ascendant ? ProductsList.OrderBy(o => o.ShippingPrice).ToList() : ProductsList.OrderByDescending(o => o.ShippingPrice).ToList();
                    break;
                case "Stock":
                    ProductsList = Ascendant ? ProductsList.OrderBy(o => o.Stock).ToList() : ProductsList.OrderByDescending(o => o.Stock).ToList();
                    break;
                default:
                    break;
            }

            // Calcular el número de páginas y elementos por página
            var totalItemsCount = ProductsList.Count;
            var totalPages = (int)Math.Ceiling((double)totalItemsCount / pageSize);

            // Obtener los elementos correspondientes a la página actual
            var itemsOnPage = ProductsList.Skip((page) * pageSize).Take(pageSize).ToList();

            // Insertar los productos en la lista
            InsertProductsIntoList(itemsOnPage);

            // Actualizar el número total de páginas
            TotalPagesCount = totalPages;

            // Actualizar la página actual
            CurrentPageIndex = page;

            OnPropertyChanged(nameof(PageSummary));
            OnPropertyChanged(nameof(TotalItemsCount));
        }
        catch (Exception ex)
        {
            await ContentDialogHelper.ShowExceptionDialog(ex, XamlRoot);
        }
    }
    [RelayCommand]
    private async void SelectMultiple()
    {
        if (ListViewProducts == null) return;

        var AllItems = ListViewProducts.Items.Count;
        if (AllItems > 0)
        {
            if (SelectMultipleIsEnabled == false)
            {
                await ShowButtons();
            }
            else if (SelectMultipleIsEnabled == true)
            {
                await HideButtons();
            }
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
                    return;
                }
            }
        }
        if (ListViewProducts.SelectedIndex != -1)
        {
            ListViewCollection.RemoveAt(ListViewProducts.SelectedIndex);
        }
    }
    public async void InsertProductsIntoList(List<ProductInfo> Products)
    {
        try
        {
            ListViewCollection.Clear();
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
            {
                var deleteCommand = new StandardUICommand(StandardUICommandKind.Delete);
                deleteCommand.ExecuteRequested += DeleteCommand_ExecuteRequested;

                foreach (var item in Products)
                {
                    var listItemData = new ProductListItem
                    {
                        Id = item.Id,
                        Title = item.Name,
                        ImageLocation = item.Image,
                        Price = item.Price.ToString(),
                        Stock = item.Stock == 0 ? "Stock Empty" : item.Stock == null ? "Not available" : item.Stock.ToString(),
                        Shipping = item.ShippingPrice == 0 ? "Free shipping" : item.ShippingPrice == null ? "Not available" : item.ShippingPrice.ToString(),
                        Command = deleteCommand
                    };

                    ListViewCollection.Add(listItemData);
                }
            }
            else
            {
                foreach (var item in Products)
                {
                    var listItemData = new ProductListItem
                    {
                        Id = item.Id,
                        Title = item.Name,
                        ImageLocation = item.Image,
                        Price = item.Price.ToString(),
                        Stock = item.Stock == 0 ? "Stock Empty" : item.Stock == null ? "Not available" : item.Stock.ToString(),
                        Shipping = item.ShippingPrice == 0 ? "Free shipping" : item.ShippingPrice == null ? "Not available" : item.ShippingPrice.ToString(),
                        Command = null
                    };

                    ListViewCollection.Add(listItemData);
                }
            }
            // Actualizar el total de ítems en la lista
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
        ClipboardAutomatically = await _clipboardSelectorService.LoadClipboardSettingFromSettingsAsync();

        if (!await IsUrlClipboardValid() && !ClipboardAutomatically) return;

        var urlClipboard = await GetClipboardTextAsync();
        if (await IsRegistered(urlClipboard)) return;

        var urlShop = await GetStoreUrlsByUrl(urlClipboard);
        if (urlShop.Count == 0 || urlShop == null) return;

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
    public async Task SearchInListView()
    {
        var Products = await App.PriceTrackerService.GetAllWithChildrenAsync<ProductInfo>();
        if (TextBoxSearchListView == null) return;

        var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
        ProductsList = Products.Where(p => compareInfo.IndexOf(p.Name, TextBoxSearchListView, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) >= 0).ToList();

        InsertProductsIntoList(ProductsList);

        var isAscending = (previousSelectedSortDirection == "Ascending");
        await GetOrderedList(previousSelectedSortBy, isAscending, CurrentPageIndex, SelectedRowsPerPageOption);

        OnPropertyChanged(nameof(PageSummary));
        OnPropertyChanged(nameof(TotalItemsCount));
    }
    [RelayCommand]
    public async Task AdvancedSearch()
    {
        try
        {
            if (await Url.IsValid(TextBoxSearch))
            {
                await SearchUrlAsync(TextBoxSearch);
            }
            else
            {
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
            var productList = await App.PriceTrackerService.GetAllWithChildrenAsync<ProductInfo>();
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
            if (ListViewProducts != null)
            {
                ListViewProducts.SelectedItem = null;
                ListViewProducts.SelectionMode = ListViewSelectionMode.Single;
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
            if (ListViewProducts != null)
            {
                ListViewProducts.SelectionMode = ListViewSelectionMode.Multiple;
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
            var AllItems = ListViewProducts.Items.Count;
            if (ListViewProducts.SelectionMode == ListViewSelectionMode.Multiple || ListViewProducts.SelectionMode == ListViewSelectionMode.Extended)
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
            if (ListViewProducts.SelectionMode == ListViewSelectionMode.Single && selectedItems.Count > 0)
            {
                var obj = (ProductListItem)ListViewProducts.SelectedItem;
                _navigationService.NavigateTo(typeof(ProductDetailsViewModel).FullName!, obj.Id);
            }
        }
        catch (Exception ex)
        {
            await ContentDialogHelper.ShowExceptionDialog(ex, XamlRoot);
        }
    }
}
