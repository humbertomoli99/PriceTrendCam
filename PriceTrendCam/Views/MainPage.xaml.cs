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
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
        Loaded += MainPage_Loaded;
        object[] objects = { ListProducts };
        DataContext = new MainViewModel(objects);
    }
    private async void MainPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.XamlRoot = this.XamlRoot;
    }

    private async void Page_GotFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.ShowMessageAddProductFromClipboard();
    }
    private void CheckBox_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ListProducts.SelectionMode == ListViewSelectionMode.Multiple || ListProducts.SelectionMode == ListViewSelectionMode.Extended)
        {
            CheckBox1.IsChecked = true;
            CheckBox1Icon.Glyph = "\ue73a";
            ListProducts.SelectAll();
        }
    }

    private void CheckBox_Unchecked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ListProducts.SelectionMode == ListViewSelectionMode.Multiple || ListProducts.SelectionMode == ListViewSelectionMode.Extended)
        {
            CheckBox1.IsChecked = false;
            CheckBox1Icon.Glyph = "\ue739";
            ListProducts.DeselectRange(new ItemIndexRange(0, (uint)ListProducts.Items.Count));
        }
    }

    private void ListProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        int itemsSelected = ListProducts.SelectedItems.Count;
        int AllItems = ListProducts.Items.Count;
        if (ListProducts.SelectionMode == ListViewSelectionMode.Multiple || ListProducts.SelectionMode == ListViewSelectionMode.Extended)
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
        //if (ListProducts.SelectionMode == ListViewSelectionMode.Single && ListProducts.SelectedItem != null)
        //{
        //    ProductsModel obj = (ProductsModel)ListProducts.SelectedItem;
        //    selectors.SelectedProduct = obj.ID_PRODUCT;
        //    NavigationService.Navigate(typeof(ProductDetailsPage));
        //}
    }
}
