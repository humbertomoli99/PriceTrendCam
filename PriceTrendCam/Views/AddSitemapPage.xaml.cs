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
        ViewModel = App.GetService<AddSitemapViewModel>();
        InitializeComponent();
    }
}
