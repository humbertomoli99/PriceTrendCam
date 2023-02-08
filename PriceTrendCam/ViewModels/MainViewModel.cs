using CommunityToolkit.Mvvm.ComponentModel;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Core.Services;

namespace PriceTrendCam.ViewModels;

public class MainViewModel : ObservableRecipient
{
    public MainViewModel()
    {
        GetDataProducts();
    }
    public static async void GetDataProducts()
    {
        var producto1 = new ProductInfo()
        {
            Id= 3,
            Name = "producto1",
            Description = "producto de prueba",
            Date= DateTime.Now,
            ShippingPrice = 100
        };

        await App.PriceTrackerService.InsertAsync<ProductInfo>(producto1);

        var data = await App.PriceTrackerService.GetAllAsync<ProductInfo>();

    }
}
