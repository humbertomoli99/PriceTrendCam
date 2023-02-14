using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Core.Services;
using Windows.Storage;

namespace PriceTrendCam.ViewModels;

public class AddSitemapViewModel : ObservableRecipient
{
    private readonly TextBox _StoreName;
    private readonly TextBox _StoreURL;
    public AddSitemapViewModel(object[] campos)
    {
        _StoreName = (TextBox)campos[0];
        _StoreURL = (TextBox)campos[1];
    }
    public AddSitemapViewModel()
    {
    }
    public ICommand ICommandAddStore => new RelayCommand(new Action(async () => await AddStoreAsync()));

    public async Task AddStoreAsync()
    {
        var store = new Store
        {
            Name = _StoreName.Text,
            Favicon = "URL del favicon",
            Urls = new List<StoreUrl>(),
            Selectors = new List<Selector>()
        };

        _ = await App.PriceTrackerService.InsertAsync(store);

        var storeUrls = new StoreUrl
        {
            StoreId = store.Id,
            Url = _StoreURL.Text,
        };

        _ = await App.PriceTrackerService.InsertAsync(storeUrls);

        var selector = new Selector
        {
            Name = _StoreName.Text,
            StoreId = store.Id,
        };

        _ = await App.PriceTrackerService.InsertAsync(selector);

        await App.PriceTrackerService.CloseAsync();
    }
}
