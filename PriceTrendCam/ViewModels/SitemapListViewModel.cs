using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.Contracts.ViewModels;
using PriceTrendCam.Core.Contracts.Services;
using PriceTrendCam.Core.Models;

namespace PriceTrendCam.ViewModels;

public partial class SitemapListViewModel : ObservableRecipient, INavigationAware
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
    [RelayCommand]
    private async void DeleteStore(XamlRoot root)
    {
        ContentDialog dialog = new()
        {
            Title = "Delete Store",
            XamlRoot = root,
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Close,
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Cancel",
            Content = "Are you sure you want to delete the store?"
        };

        await dialog.ShowAsync().AsTask();

        if (dialog.IsPrimaryButtonEnabled)
        {
            await App.PriceTrackerService.DeleteAsync(Selected);

            SampleItems.Clear();

            // TODO: Replace with real data.
            var data = await _sampleDataService.GetListDetailsDataAsync();

            foreach (var item in data)
            {
                SampleItems.Add(item);
            }
            EnsureItemSelected();
        }
    }
}
