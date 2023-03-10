using PriceTrendCam.Core.Contracts.Services;
using PriceTrendCam.Core.Models;

namespace PriceTrendCam.Services.DataService;
public class ProductInfoDataService : ISampleDataService<ProductInfo>
{
    private List<ProductInfo>? _allProducts;

    public ProductInfoDataService()
    {
    }

    public async Task<IEnumerable<ProductInfo>> AllProducts()
    {
        var data = await App.PriceTrackerService.GetAllWithChildrenAsync<ProductInfo>();
        return data.AsEnumerable();
    }

    public async Task<IEnumerable<ProductInfo>> GetContentGridDataAsync(int count = 10, int startIndex = 0)
    {
        _allProducts = new List<ProductInfo>(await AllProducts());
        return _allProducts.Skip(startIndex).Take(count);
    }

    public async Task<IEnumerable<ProductInfo>> GetGridDataAsync(int count = 10, int startIndex = 0)
    {
        _allProducts = new List<ProductInfo>(await AllProducts());
        return _allProducts.Skip(startIndex).Take(count);
    }

    public async Task<IEnumerable<ProductInfo>> GetListDetailsDataAsync(int count = 10, int startIndex = 0)
    {
        _allProducts = new List<ProductInfo>(await AllProducts());
        return _allProducts.Skip(startIndex).Take(count);
    }
}
