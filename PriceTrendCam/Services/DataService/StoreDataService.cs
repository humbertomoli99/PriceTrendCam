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
        var data = await App.PriceTrackerService.GetAllAsync<Store>();
        return data.AsEnumerable();
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
