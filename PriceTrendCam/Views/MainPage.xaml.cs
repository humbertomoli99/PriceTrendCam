using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.Core.Models;
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
    }
    private async void MainPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.XamlRoot = this.XamlRoot;
    }

    private async void Page_GotFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.ShowMessageAddProductFromClipboard();
    }
}
