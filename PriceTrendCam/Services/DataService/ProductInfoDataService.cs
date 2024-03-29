﻿using PriceTrendCam.Core.Contracts.Services;
using PriceTrendCam.Core.Models;

namespace PriceTrendCam.Services.DataService;
public class ProductInfoDataService : ISampleDataService<ProductInfo>
{
    private List<ProductInfo> _allProducts;

    public ProductInfoDataService()
    {
    }

    public async Task<IEnumerable<ProductInfo>> AllOrders()
    {
        var data = await App.PriceTrackerService.GetAllAsync<ProductInfo>();
        return data.AsEnumerable();
    }

    public async Task<IEnumerable<ProductInfo>> GetContentGridDataAsync()
    {

        if (_allProducts == null)
        {
            _allProducts = new List<ProductInfo>(await AllOrders());
        }

        await Task.CompletedTask;
        return _allProducts;
    }

    public async Task<IEnumerable<ProductInfo>> GetGridDataAsync()
    {
        if (_allProducts == null)
        {
            _allProducts = new List<ProductInfo>(await AllOrders());
        }

        await Task.CompletedTask;
        return _allProducts;
    }
    public async Task<IEnumerable<ProductInfo>> GetListDetailsDataAsync()
    {

        if (_allProducts == null)
        {
            _allProducts = new List<ProductInfo>(await AllOrders());
        }

        await Task.CompletedTask;
        return _allProducts;
    }
}
