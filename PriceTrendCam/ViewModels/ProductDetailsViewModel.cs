using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.Contracts.ViewModels;
using PriceTrendCam.Core.Contracts.Services;
using PriceTrendCam.Core.Models;

namespace PriceTrendCam.ViewModels;

public partial class ProductDetailsViewModel : ObservableRecipient, INavigationAware
{
    private readonly ISampleDataService<ProductInfo> _sampleDataService;
    private ProductInfo? _selected;

    public ProductInfo? Selected
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
    public ObservableCollection<ProductInfo> SampleItems { get; private set; } = new ObservableCollection<ProductInfo>();

    public ProductDetailsViewModel(ISampleDataService<ProductInfo> sampleDataService)
    {
        _sampleDataService = sampleDataService;
        StartIndex = 0;
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
            if (SampleItems != null && SampleItems.Count > 0)
            {
                Selected = SampleItems.FirstOrDefault();
            }
            else
            {
                // Si SampleItems es null o está vacía, no hay ningún elemento que seleccionar.
                // Aquí puedes establecer el valor predeterminado para Selected o lanzar una excepción.
            }
        }
    }
    [RelayCommand]
    private async void DeleteProduct(XamlRoot root)
    {
        ContentDialog deleteProductDialog = new()
        {
            Title = "Delete Store",
            XamlRoot = root,
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Cancel",
            Content = "Are you sure you want to delete the Product?"
        };

        ContentDialogResult dialogResult = await deleteProductDialog.ShowAsync();

        if (dialogResult == ContentDialogResult.Primary)
        {
            await App.PriceTrackerService.DeleteAsync(Selected);

            SampleItems.Clear();

            // TODO: Replace with real data.
            var listDetailsData = await _sampleDataService.GetListDetailsDataAsync();

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
