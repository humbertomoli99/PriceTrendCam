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

    public ObservableCollection<ProductInfo> SampleItems { get; private set; } = new ObservableCollection<ProductInfo>();

    public ProductDetailsViewModel(ISampleDataService<ProductInfo> sampleDataService)
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
        ContentDialog dialog = new()
        {
            Title = "Delete Store",
            XamlRoot = root,
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Close,
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Cancel",
            Content = "Are you sure you want to delete the Product?"
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
