using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Controls;

using PriceTrendCam.ViewModels;

namespace PriceTrendCam.Views;

public sealed partial class SitemapListPage : Page
{
    public SitemapListViewModel ViewModel
    {
        get;
    }

    public SitemapListPage()
    {
        ViewModel = App.GetService<SitemapListViewModel>();
        InitializeComponent();
    }
    private void OnViewStateChanged(object sender, ListDetailsViewState e)
    {
        if (e == ListDetailsViewState.Both)
        {
            ViewModel.EnsureItemSelected();
        }
    }
}
