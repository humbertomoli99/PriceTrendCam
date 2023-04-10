using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.Core.Models;
using PriceTrendCam.ViewModels;

namespace PriceTrendCam.Views;

public class ListItemData2
{
    public int Id
    {
        get; set;
    }
    public string Title
    {
        get; set;
    }
    public string Description
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
    readonly ObservableCollection<ListItemData2> collection = new();

    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
        Loaded += MainPage_Loaded;
    }
    public void LoadProductsIntoList(List<ProductInfo> Products)
    {
        collection.Clear();
        foreach (var item in Products)
        {
            var listProductsItem = new ListItemData2
            {
                Id = item.Id,
                Title = item.Name,
                Description = item.Description,
                ImageLocation = item.Image,
                Price = item.Price.ToString(),
                Stock = item.Stock.ToString(),
                Shipping = item.ShippingPrice.ToString(),
            };

            collection.Add(listProductsItem);
        }
    }
    private async void MainPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.XamlRoot = this.XamlRoot;
        var products = await App.PriceTrackerService.GetAllWithChildrenAsync<ProductInfo>();
        LoadProductsIntoList(products);
    }

    private async void Page_GotFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.ShowMessageAddProductFromClipboard();
    }
}
