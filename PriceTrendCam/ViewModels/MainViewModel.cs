using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PriceTrendCam.Core.Helpers;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Core.Services;


namespace PriceTrendCam.ViewModels;
public partial class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        AdvancedSearchCommand = new AsyncRelayCommand(AdvancedSearchAsync);
    }
    public IAsyncRelayCommand AdvancedSearchCommand
    {
        get;
    }
    [ObservableProperty]
    private string textBoxSearch;
    public async Task AdvancedSearchAsync()
    {
        if (await Url.IsValid(textBoxSearch))
        {
            await SearchUrlAsync(textBoxSearch);
        }
        else
        {
            await SearchTermAsync();
        }
    }
    private static async Task SearchUrlAsync(string url)
    {
        // Buscar si la URL tiene un sitemap y selectores asignados
        var productList = await App.PriceTrackerService.GetAllWithChildrenAsync<ProductInfo>();
        var isRegistered = ((productList?.Where(s => s?.Url?.Equals(url) ?? false)?.ToList().Count ?? 0) > 0);

        if (isRegistered)
        {
            Console.Write("The product is registered");
            return;
        }

        var UrlShop = (await App.PriceTrackerService.GetAllWithChildrenAsync<Store>())
            .SelectMany(s => s.Urls)
            .Where(u => url.Contains(u.Url))
            .ToList();

        var partnerStore = await App.PriceTrackerService.GetWithChildrenAsync<Store>(UrlShop.First().StoreId);

        if (partnerStore.Urls == null || partnerStore.Urls.Count == 0)
        {
            Console.Write("No selectors assigned to Store");
            return;
        }

        var node = await HtmlDocumentService.LoadPageAsync(url);

        var metaTitle = HtmlDocumentService.GetMetaTitle(node);
        var metaDescription = HtmlDocumentService.GetMetaDescription(node);
        var metaImage = HtmlDocumentService.GetMetaImage(node);

        var newProduct = new ProductInfo
        {
            Name = metaTitle,
            Description = metaDescription,
            Url = url,
            Price = 100.0,
            PriceCurrency = "MXN",
            Date = DateTime.UtcNow,
            ShippingPrice = 119,
            ShippingCurrency = "MXN",
            StoreName = partnerStore.Name,
            Status = "In stock",
            Stock = 100,
            Image = metaImage,
            StoreId = partnerStore.Id,
        };

        await App.PriceTrackerService.InsertAsync(newProduct);
    }
    private static async Task SearchTermAsync()
    {
        // Utilizar los diccionarios ParseWebSiteJsonLdForSearchAction para obtener una URL de búsqueda y obtener el buscador de la web
        // Si no hay ningún sitemap, informar al usuario de cómo realizar este proceso
        var productList = await App.PriceTrackerService.GetAllAsync<ProductInfo>();
        // Si no hay buscadores, solo decir al usuario los productos que tiene en seguimiento
    }

}
