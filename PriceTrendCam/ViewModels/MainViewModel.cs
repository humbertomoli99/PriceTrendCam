using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HtmlAgilityPack;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Core.Services;


namespace PriceTrendCam.ViewModels;
public partial class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
    }
    [ObservableProperty]
    private string textBoxSearch;

    private HtmlNode document;

    public XamlRoot XamlRoot
    {
        get;
        set;
    }

    [RelayCommand]
    public async Task AdvancedSearch()
    {
        try
        {
            if (await Core.Helpers.Url.IsValid(textBoxSearch))
            {
                await SearchUrlAsync(textBoxSearch);
            }
            else
            {
                await SearchTermAsync();
            }
        }
        catch (Exception ex)
        {
            // Manejo de excepciones
            var dialog = new ContentDialog
            {
                Title = "Error",
                XamlRoot = XamlRoot,
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Close,
                Content = "An error occurred: " + ex.Message
            };

            await dialog.ShowAsync();
        }
    }
    //Funciones auxiliares
    private string? GetValue(string cssSelector, string attribute)
    {
        return HtmlDocumentService.GetMetaValue(document, cssSelector, attribute);
    }
    private string? ApplyRegex(string value, string pattern, string replacement)
    {
        replacement ??= string.Empty;
        return Regex.Replace(value, pattern, replacement);
    }
    private async Task SearchUrlAsync(string url)
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

        var listSelectors = partnerStore.Selectors.ToList();
        var newProduct = new ProductInfo();
        document = await HtmlDocumentService.LoadPageAsync(url);

        foreach (var selector in listSelectors)
        {
            if (Enum.TryParse(selector.Type, out SelectorType selectorTypeEnum))
            {
                switch (selectorTypeEnum)
                {
                    case SelectorType.Title:
                        var title = GetValue(selector.CssSelector, selector.Attribute);
                        if (title == null) continue;

                        if (!string.IsNullOrEmpty(selector.Pattern))
                        {
                            title = ApplyRegex(title, selector.Pattern, selector.Replacement);
                        }
                        newProduct.Name = title;
                        break;
                    case SelectorType.Description:
                        var description = GetValue(selector.CssSelector, selector.Attribute);
                        if (description == null) continue;

                        if (!string.IsNullOrEmpty(selector.Pattern))
                        {
                            description = ApplyRegex(description, selector.Pattern, selector.Replacement);
                        }
                        newProduct.Description = description;
                        break;
                    case SelectorType.Image:
                        var image = GetValue(selector.CssSelector, selector.Attribute);
                        if (image == null) continue;

                        if (!string.IsNullOrEmpty(selector.Pattern) && image != null)
                        {
                            image = ApplyRegex(image, selector.Pattern, selector.Replacement);
                        }
                        newProduct.Image = image;
                        //si se detiene de este punto no quiero q se ejecuten los regex del siguiente buckle
                        break;
                    case SelectorType.Price:
                        var price = GetValue(selector.CssSelector, selector.Attribute);
                        if (price == null) continue;

                        if (!string.IsNullOrEmpty(selector.Pattern))
                        {
                            price = ApplyRegex(price, selector.Pattern, selector.Replacement);
                        }
                        newProduct.Price = Convert.ToDouble(price);
                        break;
                    case SelectorType.PriceCurrency:
                        var PriceCurrency = GetValue(selector.CssSelector, selector.Attribute);
                        if (PriceCurrency == null) continue;

                        if (!string.IsNullOrEmpty(selector.Pattern))
                        {
                            PriceCurrency = ApplyRegex(PriceCurrency, selector.Pattern, selector.Replacement);
                        }
                        newProduct.PriceCurrency = PriceCurrency;
                        break;
                    case SelectorType.Shipping:
                        var ShippingPrice = GetValue(selector.CssSelector, selector.Attribute);
                        if (ShippingPrice == null) continue;
                        if (!string.IsNullOrEmpty(selector.Pattern))
                        {
                            ShippingPrice = ApplyRegex(ShippingPrice, selector.Pattern, selector.Replacement);
                        }
                        newProduct.ShippingPrice = Convert.ToDouble(ShippingPrice);
                        break;
                    case SelectorType.ShippingCurrency:
                        var ShippingCurrency = GetValue(selector.CssSelector, selector.Attribute);
                        if (ShippingCurrency == null) continue;

                        if (!string.IsNullOrEmpty(selector.Pattern))
                        {
                            ShippingCurrency = ApplyRegex(ShippingCurrency, selector.Pattern, selector.Replacement);
                        }
                        newProduct.ShippingCurrency = ShippingCurrency;
                        break;
                    case SelectorType.Stock:
                        var Stock = GetValue(selector.CssSelector, selector.Attribute);
                        if (Stock == null) continue;

                        if (!string.IsNullOrEmpty(selector.Pattern))
                        {
                            Stock = ApplyRegex(Stock, selector.Pattern, selector.Replacement);
                        }
                        newProduct.Stock = Convert.ToDouble(Stock);
                        break;
                    // agregar más casos para cada tipo de selector que quieras iterar
                    default:
                        // manejo del caso predeterminado (si corresponde)
                        break;
                }
            }
        }

        newProduct.StoreId = partnerStore.Id;
        newProduct.Status = ProductStatus.Active;
        newProduct.Url = url;
        newProduct.StoreName = partnerStore.Name;
        newProduct.Date = DateTime.UtcNow;
        //newProduct.Image = GetValue("head > title", "innerHTML");
        newProduct.PriceCurrency = "MXN";
        newProduct.ShippingCurrency = "MXN";

        var isSucces = await App.PriceTrackerService.InsertAsync(newProduct);
        var message = "";
        var content = "";
        if (isSucces == 1)
        {
            message = "Product Inserted";
            content = newProduct.Name + "\n" + newProduct.Price + "\n" + newProduct.Stock;
        }
        else
        {
            message = "Not Inserted";
            content = "The product has not add";
        }
        // El producto ha sido agregado
        var dialog = new ContentDialog
        {
            Title = message,
            XamlRoot = XamlRoot,
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Close,
            Content = content
        };

        await dialog.ShowAsync();
    }
    private static async Task SearchTermAsync()
    {
        // Utilizar los diccionarios ParseWebSiteJsonLdForSearchAction para obtener una URL de búsqueda y obtener el buscador de la web
        // Si no hay ningún sitemap, informar al usuario de cómo realizar este proceso
        var productList = await App.PriceTrackerService.GetAllAsync<ProductInfo>();
        // Si no hay buscadores, solo decir al usuario los productos que tiene en seguimiento
    }

}
