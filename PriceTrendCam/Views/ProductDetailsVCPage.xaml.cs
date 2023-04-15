using CommunityToolkit.WinUI.UI.Controls;

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
        InitializeComponent();
    }
}
