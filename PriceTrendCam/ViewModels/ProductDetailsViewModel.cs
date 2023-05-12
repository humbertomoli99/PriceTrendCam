using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
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

    [ObservableProperty]
    private string productDate;

    [ObservableProperty]
    private string productId;

    [ObservableProperty]
    private ObservableCollection<string> listImages;

    public ProductDetailsViewModel()
    {
        ListImages = new ObservableCollection<string>();
    }

    public void OnNavigatedFrom()
    {

    }
    public async Task OnNavigatedTo(object parameter)
    {
        if (parameter is string stringValue && !string.IsNullOrEmpty(stringValue))
        {
            var idProduct = int.Parse(stringValue);
            await LoadProductsAsync(idProduct);
        }
        else
        {
            // Manejar el caso cuando el parámetro no es un string válido
        }
    }
    /*
   This method shows the text for the last edition based on the provided product date.
   It calculates the time difference between the current time and the product date,
   and assigns the appropriate text to the variable ProductDate.
*/

    private async Task ShowTextLastEdition(DateTime productDate)
    {
        DateTime currentTime = DateTime.Now;

        TimeSpan timeDifference = currentTime - productDate;

        if (timeDifference.TotalMinutes < 1)
        {
            ProductDate = "Less than a minute ago";
        }
        else if (timeDifference.TotalHours < 1)
        {
            var minutes = (int)timeDifference.TotalMinutes;
            ProductDate = $"About {minutes} {(minutes == 1 ? "minute" : "minutes")} ago";
        }
        else if (timeDifference.TotalDays < 1)
        {
            var hours = (int)timeDifference.TotalHours;
            ProductDate = $"About {hours} {(hours == 1 ? "hour" : "hours")} ago";
        }
        else if (timeDifference.TotalDays < 7)
        {
            var days = (int)timeDifference.TotalDays;
            ProductDate = $"About {days} {(days == 1 ? "day" : "days")} ago";
        }
        else if (timeDifference.TotalDays < 30)
        {
            var weeks = (int)(timeDifference.TotalDays / 7);
            ProductDate = $"About {weeks} {(weeks == 1 ? "week" : "weeks")} ago";
        }
        else
        {
            var months = (int)(timeDifference.TotalDays / 30);
            ProductDate = $"Last edited: {months} {(months == 1 ? "month" : "months")} ago";
        }
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
        ProductId = Product.Id_item;

        var Histories = await App.PriceTrackerService.GetAllAsync<History>();
        var ProductHistoryList = Histories.Where(u => u.ProductInfoId.Equals(Product.Id)).ToList();

        var NumberOfRecords = ProductHistoryList.Count - 1;

        var SumProductPrice = new double?[ProductHistoryList.Count];

        await ShowTextLastEdition(ProductHistoryList[NumberOfRecords].Date);

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
