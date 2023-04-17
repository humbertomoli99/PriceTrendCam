using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.Contracts.ViewModels;
using PriceTrendCam.Core.Contracts.Services;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Helpers;

namespace PriceTrendCam.ViewModels;

public partial class ProductDetailsVCViewModel : ObservableRecipient, INavigationAware
{
    private readonly ISampleDataService<ProductInfo> _sampleDataService;
    private ProductInfo? _selected;
    public XamlRoot XamlRoot
    {
        get; set;
    }
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

    public ProductDetailsVCViewModel(ISampleDataService<ProductInfo> sampleDataService)
    {
        _sampleDataService = sampleDataService;
        StartIndex = 0;
    }

    public async Task OnNavigatedTo(object parameter)
    {
        await LoadDataAsync(StartIndex);
    }
    private async Task LoadDataAsync(int startIndex)
    {
        SampleItems.Clear();

        var data = await _sampleDataService.GetListDetailsDataAsync(count: 10, startIndex: startIndex);

        foreach (var item in data)
        {
            item.Image ??= "ms-appx:///Assets/Mountain_Monochromatic.png";
            item.Description = item.Description.Replace("\n", "<LineBreak />");
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
        ContentDialog deleteProductDialog = new()
        {
            Title = "Delete Store",
            XamlRoot = XamlRoot,
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            Content = "Are you sure you want to delete the Product?"
        };

        ContentDialogResult dialogResult = await ContentDialogHelper<ContentDialog>.Instance.ShowContentDialog(deleteProductDialog);

        if (dialogResult == ContentDialogResult.Primary)
        {
            _ = await App.PriceTrackerService.DeleteAsync<ProductInfo>(Selected.Id);

            await LoadDataAsync(StartIndex);
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

        await LoadDataAsync(StartIndex);
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

        await LoadDataAsync(StartIndex);
    }
}
