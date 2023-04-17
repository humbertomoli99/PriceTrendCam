using Microsoft.UI.Xaml.Controls;

using PriceTrendCam.ViewModels;

namespace PriceTrendCam.Views;

public sealed partial class ProductDetailsVCPage : Page
{
    public ProductDetailsVCViewModel ViewModel
    {
        get;
    }

    public ProductDetailsVCPage()
    {
        ViewModel = App.GetService<ProductDetailsVCViewModel>();
        Loaded += ProductDetailsVCPage_Loaded;
        InitializeComponent();
    }

    private void ProductDetailsVCPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.XamlRoot = XamlRoot;
    }
}
