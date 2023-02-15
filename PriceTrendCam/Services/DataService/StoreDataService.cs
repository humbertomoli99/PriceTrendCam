using PriceTrendCam.Core.Contracts.Services;
using PriceTrendCam.Core.Models;
using Windows.Services.Store;

namespace PriceTrendCam.Services.DataService;
public class StoreDataService : ISampleDataService<Store>
{
    private List<Store> _allProducts;

    public StoreDataService()
    {
    }

    public async Task<IEnumerable<Store>> AllProducts()
    {
        var stores = await App.PriceTrackerService.GetAllAsync<Store>();
        var selectors = await App.PriceTrackerService.GetAllAsync<Selector>();
        var storeUrls = await App.PriceTrackerService.GetAllAsync<StoreUrl>();

        foreach (var store in stores)
        {
            store.Selectors = selectors.Where(x => x.StoreId == store.Id).ToList();
            store.Urls = storeUrls.Where(x => x.StoreId == store.Id).ToList();
        }
        await App.PriceTrackerService.CloseAsync();
        return stores.AsEnumerable();
    }

    public async Task<IEnumerable<Store>> GetContentGridDataAsync()
    {
        _allProducts = new List<Store>(await AllProducts());
        return _allProducts;
    }

    public async Task<IEnumerable<Store>> GetGridDataAsync()
    {
        _allProducts = new List<Store>(await AllProducts());
        return _allProducts;
    }
    public async Task<IEnumerable<Store>> GetListDetailsDataAsync()
    {
        _allProducts = new List<Store>(await AllProducts());
        return _allProducts;
    }
}
