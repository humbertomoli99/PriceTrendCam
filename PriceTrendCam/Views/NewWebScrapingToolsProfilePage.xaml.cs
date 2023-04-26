using Microsoft.UI.Xaml.Controls;

using PriceTrendCam.ViewModels;

namespace PriceTrendCam.Views;

public sealed partial class NewWebScrapingToolsProfilePage : Page
{
    public NewWebScrapingToolsProfileViewModel ViewModel
    {
        get;
    }

    public NewWebScrapingToolsProfilePage()
    {
        ViewModel = App.GetService<NewWebScrapingToolsProfileViewModel>();
        InitializeComponent();
    }
}
