using Microsoft.UI.Xaml.Controls;

using PriceTrendCam.ViewModels;

namespace PriceTrendCam.Views;

public sealed partial class ProductsListPage : Page
{
    public ProductsListViewModel ViewModel
    {
        get;
    }

    public ProductsListPage()
    {
        ViewModel = App.GetService<ProductsListViewModel>();
        InitializeComponent();
    }
}
