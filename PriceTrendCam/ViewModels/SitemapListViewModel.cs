using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.Contracts.Services;
using PriceTrendCam.Contracts.ViewModels;
using PriceTrendCam.Core.Contracts.Services;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Helpers;

namespace PriceTrendCam.ViewModels;

public partial class SitemapListViewModel : ObservableRecipient, INavigationAware
{
    private readonly ISampleDataService<Store> _sampleDataService;
    private Store? _selected;
    private readonly INavigationService navigationService;

    public Store? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }
    private int _startIndex;
    public int StartIndex
    {
        get => _startIndex;
        set => SetProperty(ref _startIndex, value);
    }

    public ObservableCollection<Store> SampleItems { get; private set; } = new ObservableCollection<Store>();

    public SitemapListViewModel(ISampleDataService<Store> sampleDataService, INavigationService navigationService)
    {
        _sampleDataService = sampleDataService;
        this.navigationService = navigationService;
        StartIndex = 0;
    }
    public async Task OnNavigatedTo(object parameter)
    {
        SampleItems.Clear();

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
    private async void EditStore()
    {
        navigationService.NavigateTo(typeof(AddSelectorsViewModel).FullName!, Selected.Id);
    }
    [RelayCommand]
    private async void DeleteStore(XamlRoot root)
    {
        ContentDialog deleteStoreDialog = new()
        {
            Title = "Delete Store",
            XamlRoot = root,
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            Content = "Are you sure you want to delete the store?"
        };

        ContentDialogResult dialogResult = await ContentDialogHelper<ContentDialog>.Instance.ShowContentDialog(deleteStoreDialog);

        if (dialogResult == ContentDialogResult.Primary)
        {
            _ = await App.PriceTrackerService.DeleteAsync<Store>(Selected.Id);

            SampleItems.Clear();

            var listDetailsData = await _sampleDataService.GetListDetailsDataAsync(startIndex: StartIndex);

            foreach (var item in listDetailsData)
            {
                SampleItems.Add(item);
            }
            EnsureItemSelected();
        }
    }
    [RelayCommand]
    private async void Back()
    {
        if (StartIndex <= 0)
        {
            return;
        }
        else
        {
            StartIndex--;
        }
        SampleItems.Clear();

        var data = await _sampleDataService.GetListDetailsDataAsync(count: 10, startIndex: StartIndex);

        foreach (var item in data)
        {
            SampleItems.Add(item);
        }
        EnsureItemSelected();
    }
    [RelayCommand]
    private async void Forward()
    {
        var DataMaxCount = await _sampleDataService.GetMaxPageCountAsync() - 1;

        if (StartIndex >= DataMaxCount)
        {
            return;
        }
        else
        {
            StartIndex++;
        }

        SampleItems.Clear();
        var data = await _sampleDataService.GetListDetailsDataAsync(count: 10, startIndex: StartIndex);

        foreach (var item in data)
        {
            SampleItems.Add(item);
        }
        EnsureItemSelected();
    }
}