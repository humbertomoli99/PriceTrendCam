using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.Contracts.ViewModels;
using PriceTrendCam.Core.Models;

namespace PriceTrendCam.ViewModels;

public class ProductDetailsViewModel : ObservableRecipient, INavigationAware
{
    private int _id;
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    private int _idProduct;
    public int Id_Product
    {
        get => _idProduct;
        set => SetProperty(ref _idProduct, value);
    }

    private string _productName;
    public string ProductName
    {
        get => _productName;
        set => SetProperty(ref _productName, value);
    }

    private string _productDescription;
    public string ProductDescription
    {
        get => _productDescription;
        set => SetProperty(ref _productDescription, value);
    }

    private string _priceTag;
    public string PriceTag
    {
        get => _priceTag;
        set => SetProperty(ref _priceTag, value);
    }

    private string _shippingPrice;
    public string ShippingPrice
    {
        get => _shippingPrice;
        set => SetProperty(ref _shippingPrice, value);
    }

    private string _shippingCurrency;
    public string ShippingCurrency
    {
        get => _shippingCurrency;
        set => SetProperty(ref _shippingCurrency, value);
    }

    private string? _priceCurrency;
    public string? PriceCurrency
    {
        get => _priceCurrency;
        set => SetProperty(ref _priceCurrency, value);
    }

    private string _productUrl;
    public string ProductUrl
    {
        get => _productUrl;
        set => SetProperty(ref _productUrl, value);
    }

    private string _stock;
    public string Stock
    {
        get => _stock;
        set => SetProperty(ref _stock, value);
    }

    private string _showMinProductPrice;
    public string ShowMinProductPrice
    {
        get => _showMinProductPrice;
        set => SetProperty(ref _showMinProductPrice, value);
    }

    private string _showAvgProductPrice;
    public string ShowAvgProductPrice
    {
        get => _showAvgProductPrice;
        set => SetProperty(ref _showAvgProductPrice, value);
    }

    private string _showMaxProductPrice;
    public string ShowMaxProductPrice
    {
        get => _showMaxProductPrice;
        set => SetProperty(ref _showMaxProductPrice, value);
    }

    private string _productHistory;
    public string ProductHistory
    {
        get => _productHistory;
        set => SetProperty(ref _productHistory, value);
    }

    private ObservableCollection<string> _listImages;
    

    public ObservableCollection<string> ListImages
    {
        get => _listImages;
        set => SetProperty(ref _listImages, value);
    }

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
        ProductInfo Product = await App.PriceTrackerService.GetAsync<ProductInfo>(id);

        ProductName = Product.Name;
        ProductDescription = Product.Description;
        ShippingCurrency = Product.ShippingCurrency;
        PriceCurrency = Product.PriceCurrency;
        ProductUrl = Product.Url;
        Id_Product = Product.Id;

        var Histories = await App.PriceTrackerService.GetAllAsync<History>();
        var ProductHistoryList = Histories.Where(u => u.ProductInfoId.Equals(Product.Id)).ToList();

        var NumberOfRecords = ProductHistoryList.Count;

        var SumProductPrice = new double?[NumberOfRecords];

        var i = 0;
        foreach (var item in ProductHistoryList)
        {
            SumProductPrice[i] = double.Parse(item.Price.ToString());
            ProductHistory += "Price: " + item.Price + "  Shipping: " + item.ShippingPrice + "  Stock: " + item.Stock + "   Date: " + item.Date + "\n";
            i++;
        }

        ShowAvgProductPrice = SumProductPrice.Average().ToString();
        ShowMinProductPrice = SumProductPrice.Min().ToString();
        ShowMaxProductPrice = SumProductPrice.Max().ToString();

        //cargando galeria de imagenes
        var Images = await App.PriceTrackerService.GetAllAsync<ProductPhoto>();
        var ProductImages = Images.Where(u => u.ProductId.Equals(Product.Id)).ToList();

        var LocalState = Windows.Storage.ApplicationData.Current.LocalFolder.Path;

        ListImages = new ObservableCollection<string>() { };
        foreach (var item in ProductImages)
        {
            ListImages.Add(LocalState + item.Src);
        }

        //Data de historial de producto
        if (ProductHistoryList.Count == 0) return;
        var lastItemIndex = ProductHistoryList.Count - 1;

        //Precio de productos
        if (ProductHistoryList[lastItemIndex].Price == null)
        {
            PriceTag = "Not Available";
        }
        else
        {
            PriceTag = PriceCurrency + " " + ProductHistoryList[lastItemIndex].Price.ToString();
        }

        //Precio de envio
        if (ProductHistoryList[lastItemIndex].ShippingPrice == null)
        {
            ShippingPrice = "Not Available";
        }
        else
        {
            ShippingPrice = (ProductHistoryList[lastItemIndex].ShippingPrice <= 0) ? "Free Shipping" : ShippingCurrency + ProductHistoryList[lastItemIndex].ShippingPrice;
        }

        Stock = (ProductHistoryList[lastItemIndex].Stock == null) ? "Not Available" : ProductHistoryList[lastItemIndex].Stock.ToString();
    }
}
