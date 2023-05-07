using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.Contracts.Services;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Helpers;

namespace PriceTrendCam.Models;
public partial class MainModel : ObservableObject
{
    //Paginacion
    [ObservableProperty]
    public int currentPageIndex;

    public string PageSummary => $"Page {CurrentPageIndex + 1} of {TotalPagesCount}";

    public int _totalItemsCount;

    public int _rowsPerPage;
    public int TotalItemsCount => ListViewCollection.Count;
    public int TotalPagesCount;

    public ObservableCollection<int> RowsPerPageOptions
    {
        get; set;
    }

    //busqueda
    [ObservableProperty]
    public string textBoxSearch;

    [ObservableProperty]
    public string textBoxSearchListView;

    //Order List By
    public string previousSelectedSortBy;
    public string previousSelectedSortDirection;
    public string OrderBy
    {
        get;
        set;
    }
    public bool OrderDescen;

    //lista de productos
    public List<ProductInfo> ProductsList
    {
        get;
        set;
    }

    public ObservableCollection<ProductListItem> ListViewCollection;

    public ListView ListViewProducts
    {
        get;
        set;
    }

    //Message dialog
    public XamlRoot XamlRoot
    {
        get; set;
    }

    public string message;
    public string content;

    public ContentDialogHelper<ContentDialog> ContentDialogHelper
    {
        get; set;
    }

    //navegacion servicio
    public INavigationService _navigationService;

    //Clipboard service
    public IClipboardSelectorService _clipboardSelectorService;

    public bool ClipboardAutomatically
    {
        get;
        set;
    }

    //Seleccion de productos
    [ObservableProperty]
    public Visibility isCheckedAllVisibility;

    public FontIcon SelectAllCheckBoxIcon
    {
        get;
        set;
    }

    public bool SelectMultipleIsEnabled;

    public AppBarToggleButton SelectAllCheckBox
    {
        get;
        set;
    }

    //eliminacion de productos
    [ObservableProperty]
    public Visibility deleteProductVisibility;
}