using Microsoft.UI.Xaml.Controls;

using PriceTrendCam.ViewModels;

namespace PriceTrendCam.Views;

public sealed partial class ControlPagerPage : UserControl
{
    public ControlPagerViewModel ViewModel
    {
        get;
    }

    public ControlPagerPage()
    {
        ViewModel = App.GetService<ControlPagerViewModel>();
        InitializeComponent();
    }
}
