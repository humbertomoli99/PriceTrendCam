using CommunityToolkit.Mvvm.ComponentModel;
using PriceTrendCam.Contracts.ViewModels;
using PriceTrendCam.Core.Models;

namespace PriceTrendCam.ViewModels;

public class ProductDetailsViewModel : ObservableRecipient, INavigationAware
{
    public ProductDetailsViewModel()
    {

    }

    public void OnNavigatedFrom()
    {

    }
    public async Task OnNavigatedTo(object parameter)
    {
        var idProduct = int.Parse(parameter.ToString());
        await LoadProductsAsync(idProduct);
    }
    public async Task LoadProductsAsync(int id)
    {

        // Obtener los productos de alguna fuente de datos
        ProductInfo product = await App.PriceTrackerService.GetAsync<ProductInfo>(id);

        // Insertar los productos en la lista
        //await InsertProduct(product);
    }
}
