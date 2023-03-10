using PriceTrendCam.Core.Contracts.Services;
using PriceTrendCam.Core.Models;

namespace PriceTrendCam.Services.DataService;
public class ProductInfoDataService : ISampleDataService<ProductInfo>
{
    private List<ProductInfo> _allProducts;

    public ProductInfoDataService()
    {
    }

    public async Task<IEnumerable<ProductInfo>> AllProducts()
    {
        var data = await App.PriceTrackerService.GetAllWithChildrenAsync<ProductInfo>();
        return data.AsEnumerable();
    }

    public async Task<IEnumerable<ProductInfo>> GetContentGridDataAsync(int count = 10, int pageIndex = 0)
    {
        _allProducts = new List<ProductInfo>(await AllProducts());
        int startIndex = pageIndex * count;
        return _allProducts.Skip(startIndex).Take(count);
    }

    public async Task<IEnumerable<ProductInfo>> GetGridDataAsync(int count = 10, int pageIndex = 0)
    {
        _allProducts = new List<ProductInfo>(await AllProducts());
        int startIndex = pageIndex * count;
        return _allProducts.Skip(startIndex).Take(count);
    }

    public async Task<IEnumerable<ProductInfo>> GetListDetailsDataAsync(int count = 10, int pageIndex = 0)
    {
        _allProducts = new List<ProductInfo>(await AllProducts());
        int startIndex = pageIndex * count;
        return _allProducts.Skip(startIndex).Take(count);
    }

    public async Task<int> GetMaxPageCountAsync(int count = 10)
    {
        _allProducts = new List<ProductInfo>(await AllProducts());
        int totalProducts = _allProducts.Count;
        int totalPages = (int)Math.Ceiling((double)totalProducts / count);
        return totalPages;
    }

    public Task<int> GetMaxRecordCountAsync()
    {
        return Task.FromResult(_allProducts?.Count ?? 0);
    }
}
