using Microsoft.UI.Xaml.Controls;

using PriceTrendCam.ViewModels;

namespace PriceTrendCam.Views;

public sealed partial class PackageTrackerPage : Page
{
    public PackageTrackerViewModel ViewModel
    {
        get;
    }

    public PackageTrackerPage()
    {
        ViewModel = App.GetService<PackageTrackerViewModel>();
        InitializeComponent();
    }
}
