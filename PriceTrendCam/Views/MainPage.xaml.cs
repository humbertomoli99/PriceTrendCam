using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Services;
using PriceTrendCam.ViewModels;

namespace PriceTrendCam.Views;

public class ProductListItem
{
    public int Id
    {
        get; set;
    }
    public string Title
    {
        get; set;
    }
    public string ImageLocation
    {
        get; set;
    }
    public string Price
    {
        get; set;
    }
    public string Shipping
    {
        get; set;
    }
    public string Stock
    {
        get; set;
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
        ViewModel.xamlRoot = XamlRoot;
        ViewModel.ListViewProducts = ListViewProducts;

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
