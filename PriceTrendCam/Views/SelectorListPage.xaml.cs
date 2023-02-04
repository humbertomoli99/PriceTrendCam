using Microsoft.UI.Xaml.Controls;

using PriceTrendCam.ViewModels;

namespace PriceTrendCam.Views;

public sealed partial class SelectorListPage : Page
{
    public SelectorListViewModel ViewModel
    {
        get;
    }

    public SelectorListPage()
    {
        ViewModel = App.GetService<SelectorListViewModel>();
        InitializeComponent();
    }
}
