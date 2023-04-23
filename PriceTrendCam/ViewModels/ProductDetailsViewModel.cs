using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.Contracts.ViewModels;
using PriceTrendCam.Core.Models;

namespace PriceTrendCam.ViewModels;

public partial class ProductDetailsViewModel : ObservableRecipient, INavigationAware
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private int id_Product;

    [ObservableProperty]
    private string productName;

    [ObservableProperty]
    private string productDescription;

    [ObservableProperty]
    private string priceTag;

    [ObservableProperty]
    private string shippingPrice;

    [ObservableProperty]
    private string shippingCurrency;

    [ObservableProperty]
    private string priceCurrency;

    [ObservableProperty]
    private string productUrl;

    [ObservableProperty]
    private string stock;

    [ObservableProperty]
    private string showMinProductPrice;

    [ObservableProperty]
    private string showAvgProductPrice;

    [ObservableProperty]
    private string showMaxProductPrice;

    [ObservableProperty]
    private string productHistory;

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
