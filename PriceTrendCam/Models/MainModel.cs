using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.Contracts.Services;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Helpers;
using PriceTrendCam.Views;

namespace PriceTrendCam.Models;
public partial class MainModel : ObservableObject
{
    [ObservableProperty]
    public int currentPageIndex;

    [ObservableProperty]
    public string textBoxSearch;

    [ObservableProperty]
    public string textBoxSearchListView;

    [ObservableProperty]
    public Visibility isCheckedAllVisibility;

    [ObservableProperty]
    public Visibility deleteProductVisibility;

    public string PageSummary => $"Page {CurrentPageIndex + 1} of {TotalPagesCount}";

    public ContentDialogHelper<ContentDialog> ContentDialogHelper
    {
        get; set;
    }
    public XamlRoot XamlRoot
    {
        get; set;
    }
    public int _totalItemsCount;

    public int _rowsPerPage;
    public int TotalItemsCount => ListViewCollection.Count;
    public int TotalPagesCount;
    public ObservableCollection<int> RowsPerPageOptions
    {
        get; set;
    }

    public bool SelectMultipleIsEnabled;
    public string message;
    public string content;
    public bool OrderDescen;

    public bool ClipboardAutomatically
    {
        get;
        set;
    }

    public IClipboardSelectorService _clipboardSelectorService;
    public INavigationService _navigationService;

    public ObservableCollection<ProductListItem> ListViewCollection;

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
        set;
    }
    public List<ProductInfo> ProductsList
    {
        get;
        set;
    }

    public string previousSelectedSortBy;
    public string previousSelectedSortDirection;
}
