using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Controls;

using PriceTrendCam.Contracts.Services;
using PriceTrendCam.UserControls;
using PriceTrendCam.ViewModels;
using PriceTrendCam.Views;

namespace PriceTrendCam.Services;

public class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = new();

    public PageService()
    {
        Configure<MainViewModel, MainPage>();
        Configure<ProductsListViewModel, ProductsListPage>();
        Configure<ProductDetailsVCViewModel, ProductDetailsVCPage>();
        Configure<SitemapListViewModel, SitemapListPage>();
        Configure<AddSitemapViewModel, AddSitemapPage>();
        Configure<SelectorListViewModel, SelectorListPage>();
        Configure<AddSelectorsViewModel, AddSelectorsPage>();
        Configure<ExportDataViewModel, ExportDataPage>();
        Configure<ContentGridViewModel, ContentGridPage>();
        Configure<ContentGridDetailViewModel, ContentGridDetailPage>();
        Configure<NotificationsViewModel, NotificationsPage>();
        Configure<PackageTrackerViewModel, PackageTrackerPage>();
        Configure<SettingsViewModel, SettingsPage>();
        Configure<PostManFormViewModel, PostManFormPage>();
        Configure<ProductDetailsViewModel, ProductDetailsPage>();
        Configure<NewWebScrapingToolsProfileViewModel, NewWebScrapingToolsProfilePage>();
    }

    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
            }
        }

        return pageType;
    }

    private void Configure<VM, V>()
        where VM : ObservableObject
        where V : Page
    {
        lock (_pages)
        {
            var key = typeof(VM).FullName!;
            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in PageService");
            }

            var type = typeof(V);
            if (_pages.Any(p => p.Value == type))
            {
                throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
            }

            _pages.Add(key, type);
        }
    }
}
