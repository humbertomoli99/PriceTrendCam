using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.ViewModels;

namespace PriceTrendCam.Views;

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

    private void MainPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.XamlRoot = this.XamlRoot;
    }

    private async void Page_GotFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.ShowMessageAddProductFromClipboard();
    }
}
