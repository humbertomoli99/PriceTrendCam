using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Services;
using PriceTrendCam.ViewModels;

namespace PriceTrendCam.Views;

public class ProductListItem : ObservableObject
{
    private int _id;
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    private string _title;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _imageLocation;
    public string ImageLocation
    {
        get => _imageLocation;
        set => SetProperty(ref _imageLocation, value);
    }

    private string _price;
    public string Price
    {
        get => _price;
        set => SetProperty(ref _price, value);
    }

    private string _shipping;
    public string Shipping
    {
        get => _shipping;
        set => SetProperty(ref _shipping, value);
    }

    private string _stock;
    public string Stock
    {
        get => _stock;
        set => SetProperty(ref _stock, value);
    }
}

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;set;
    }

    public MainPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<MainViewModel>();
        Loaded += MainPage_Loaded;
    }

    private async void MainPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.ListViewProducts = ListViewProducts;
        ViewModel.xamlRoot = XamlRoot;

        // Cargar los productos en la lista
        await ViewModel.LoadProductsAsync();

        // Establecer el contexto de datos del ListView
        DataContext = ViewModel;
    }

    private async void Page_GotFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.ShowMessageAddProductFromClipboard();
    }
    private void CheckBox_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ListViewProducts.SelectionMode == ListViewSelectionMode.Multiple || ListViewProducts.SelectionMode == ListViewSelectionMode.Extended)
        {
            CheckBox1.IsChecked = true;
            CheckBox1Icon.Glyph = "\ue73a";
            ListViewProducts.SelectAll();
        }
    }

    private void CheckBox_Unchecked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ListViewProducts.SelectionMode == ListViewSelectionMode.Multiple || ListViewProducts.SelectionMode == ListViewSelectionMode.Extended)
        {
            CheckBox1.IsChecked = false;
            CheckBox1Icon.Glyph = "\ue739";
            ListViewProducts.DeselectRange(new ItemIndexRange(0, (uint)ListViewProducts.Items.Count));
        }
    }

    private void ListViewProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var itemsSelected = ListViewProducts.SelectedItems.Count;
        var AllItems = ListViewProducts.Items.Count;
        if (ListViewProducts.SelectionMode == ListViewSelectionMode.Multiple || ListViewProducts.SelectionMode == ListViewSelectionMode.Extended)
        {
            if (itemsSelected == AllItems)
            {
                CheckBox1.IsChecked = true;
                CheckBox1Icon.Glyph = "\ue73a";
            }
            else if (itemsSelected == 0)
            {
                CheckBox1.IsChecked = false;
                CheckBox1Icon.Glyph = "\ue739";
            }
            else
            {
                CheckBox1.IsChecked = false;
                CheckBox1Icon.Glyph = "\uf16e";
            }
        }
        //if (ListViewProducts.SelectionMode == ListViewSelectionMode.Single && ListViewProducts.SelectedItem != null)
        //{
        //    ProductsModel obj = (ProductsModel)ListViewProducts.SelectedItem;
        //    selectors.SelectedProduct = obj.ID_PRODUCT;
        //    NavigationService.Navigate(typeof(ProductDetailsPage));
        //}
    }
}
