using PriceTrendCam.Core.Contracts.Services;
using PriceTrendCam.Core.Models;

namespace PriceTrendCam.Services.DataService;
public class StoreDataService : ISampleDataService<Store>
{
    private List<Store> _allProducts;

    public StoreDataService()
    {
    }

    public async Task<IEnumerable<Store>> AllProducts()
    {
        var data = await App.PriceTrackerService.GetAllWithChildrenAsync<Store>();
        return data.AsEnumerable();
    }

    public async Task<IEnumerable<Store>> GetContentGridDataAsync(int count = 10, int pageIndex = 0)
    {
        _allProducts = new List<Store>(await AllProducts());
        int startIndex = pageIndex * count;
        return _allProducts.Skip(startIndex).Take(count);
    }

    public async Task<IEnumerable<Store>> GetGridDataAsync(int count = 10, int pageIndex = 0)
    {
        _allProducts = new List<Store>(await AllProducts());
        int startIndex = pageIndex * count;
        return _allProducts.Skip(startIndex).Take(count);
    }

    public async Task<IEnumerable<Store>> GetListDetailsDataAsync(int count = 10, int pageIndex = 0)
    {
        _allProducts = new List<Store>(await AllProducts());
        int startIndex = pageIndex * count;
        return _allProducts.Skip(startIndex).Take(count);
    }

    public async Task<int> GetMaxPageCountAsync(int count = 10)
    {
        _allProducts = new List<Store>(await AllProducts());
        int totalProducts = _allProducts.Count;
        int totalPages = (int)Math.Ceiling((double)totalProducts / count);
        return totalPages;
    }

    public Task<int> GetMaxRecordCountAsync()
    {
        return Task.FromResult(_allProducts?.Count ?? 0);
    }
}
