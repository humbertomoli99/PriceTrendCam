using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PriceTrendCam.Contracts.ViewModels;
using PriceTrendCam.Core.Contracts.Services;
using PriceTrendCam.Core.Models;

namespace PriceTrendCam.ViewModels;

public class SitemapListViewModel : ObservableRecipient, INavigationAware
{
    private readonly ISampleDataService<Store> _sampleDataService;
    private Store? _selected;

    public Store? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    public ObservableCollection<Store> SampleItems { get; private set; } = new ObservableCollection<Store>();

    public SitemapListViewModel(ISampleDataService<Store> sampleDataService)
    {
        _sampleDataService = sampleDataService;
    }
    public async Task OnNavigatedTo(object parameter)
    {
        SampleItems.Clear();

        // TODO: Replace with real data.
        var data = await _sampleDataService.GetListDetailsDataAsync();

        foreach (var item in data)
        {
            SampleItems.Add(item);
        }
        EnsureItemSelected();
    }

    public void OnNavigatedFrom()
    {
    }

    public void EnsureItemSelected()
    {
        if (Selected == null)
        {
            Selected = SampleItems.FirstOrDefault<Store>();
        }
    }
}
