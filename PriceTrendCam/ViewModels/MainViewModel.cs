using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Core.Services;

namespace PriceTrendCam.ViewModels;

public class MainViewModel : ObservableRecipient
{
    public MainViewModel()
    {
        AdvancedSearchCommand = new AsyncRelayCommand(AdvancedSearchAsync);
    }
    public IAsyncRelayCommand AdvancedSearchCommand
    {
        get;
    }
    private async Task AdvancedSearchAsync()
    {
        var producto1 = new ProductInfo()
        {
            Id = 3,
            Name = "producto1",
            Description = "producto de prueba",
            Date = DateTime.Now,
            ShippingPrice = 100
        };

        await App.PriceTrackerService.InsertAsync<ProductInfo>(producto1);

        var data = await App.PriceTrackerService.GetAllAsync<ProductInfo>();
    }
}
