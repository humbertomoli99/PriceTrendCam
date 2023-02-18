using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PriceTrendCam.Core.Models;

namespace PriceTrendCam.ViewModels;

public partial class AddSitemapViewModel : ObservableObject
{

    public AddSitemapViewModel()
    {
        SaveCommand = new AsyncRelayCommand(Save);
        TextBoxUrl = new();
    }

    private Store? ObjectStore
    {
        get; set;
    }

    [ObservableProperty]
    private string textBoxStoreName;
    public List<string> TextBoxUrl
    {
        get; set;
    }

    public IAsyncRelayCommand SaveCommand { get; }

    private async Task Save()
    {
        var textBoxUrls = new List<StoreUrl>();

        foreach (var items in TextBoxUrl)
        {
            textBoxUrls.Add(new StoreUrl()
            {
                Url = items.ToString(),
            });
        }

        ObjectStore = new Store
        {
            Name = textBoxStoreName,
            Favicon = "favicon.png",
            Selectors = new List<Selector>(),
            Urls = textBoxUrls
        };

        await App.PriceTrackerService.InsertWithChildrenAsync<Store>(ObjectStore, true);
    }
}