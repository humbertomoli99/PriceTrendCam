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
    }

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        _ = ViewModel.AdvancedSearchAsync();
    }
}
