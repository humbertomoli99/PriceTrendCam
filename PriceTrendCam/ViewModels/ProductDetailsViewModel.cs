﻿using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    private async void DeleteProduct()
    {
        //TODO: CONFIRMAR ELIMINAR LA STORE PARA QUE NO SE BORRE INSTANTANEAMENTE
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
