using Microsoft.UI.Xaml.Controls;

using PriceTrendCam.ViewModels;

namespace PriceTrendCam.Views;

public sealed partial class AddSitemapPage : Page
{
    public AddSitemapViewModel ViewModel
    {
        get;
    }

    public AddSitemapPage()
    {
        InitializeComponent();
        object[] campos = { StoreName, StoreURL };
        ViewModel = App.GetService<AddSitemapViewModel>();
        DataContext = new AddSitemapViewModel(campos);
    }
}
