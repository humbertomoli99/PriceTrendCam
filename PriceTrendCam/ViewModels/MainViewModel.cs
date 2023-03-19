using System.Text.RegularExpressions;
using AngleSharp.Dom;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HtmlAgilityPack;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
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
        var element = HtmlDocumentService.GetMetaValue(document, cssSelector, attribute);
        if (element == null) { return string.Empty; }
        return element;
    }
    private string ApplyRegex(string value, string pattern, string replacement)
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
                        newProduct.Name = GetValue(selector.CssSelector, selector.Attribute);
                        break;
                    case SelectorType.Description:
                        newProduct.Description = GetValue(selector.CssSelector, selector.Attribute);
                        break;
                    case SelectorType.Image:
                        newProduct.Image = GetValue(selector.CssSelector, selector.Attribute);
                        break;
                    case SelectorType.Price:
                        newProduct.Price = Convert.ToDouble(GetValue(selector.CssSelector, selector.Attribute));
                        break;
                    case SelectorType.PriceCurrency:
                        newProduct.PriceCurrency = GetValue(selector.CssSelector, selector.Attribute);
                        break;
                    case SelectorType.Shipping:
                        newProduct.ShippingPrice = Convert.ToDouble(GetValue(selector.CssSelector, selector.Attribute));
                        break;
                    case SelectorType.ShippingCurrency:
                        newProduct.ShippingCurrency = GetValue(selector.CssSelector, selector.Attribute);
                        break;
                    case SelectorType.Stock:
                        newProduct.Stock = GetValue(selector.CssSelector, selector.Attribute);
                        break;
                    // agregar más casos para cada tipo de selector que quieras iterar
                    default:
                        // manejo del caso predeterminado (si corresponde)
                        break;
                }
            }
            //aplicar regex si hay un patrón y una expresión de reemplazo especificados
            if (!string.IsNullOrEmpty(selector.Pattern))
            {
                if (Enum.TryParse(selector.Type, out SelectorType selectorTypeEnum1))
                {
                    switch (selectorTypeEnum1)
                    {
                        case SelectorType.Title:
                            newProduct.Name = ApplyRegex(newProduct.Name, selector.Pattern, selector.Replacement);
                            break;
                        case SelectorType.Description:
                            newProduct.Description = ApplyRegex(newProduct.Description, selector.Pattern, selector.Replacement);
                            break;
                        case SelectorType.Image:
                            newProduct.Image = ApplyRegex(newProduct.Image, selector.Pattern, selector.Replacement);
                            break;
                        case SelectorType.Price:
                            newProduct.Price = Convert.ToDouble(ApplyRegex(newProduct.Price.ToString(), selector.Pattern, selector.Replacement));
                            break;
                        case SelectorType.PriceCurrency:
                            newProduct.PriceCurrency = ApplyRegex(newProduct.PriceCurrency, selector.Pattern, selector.Replacement);
                            break;
                        case SelectorType.Shipping:
                            newProduct.ShippingPrice = Convert.ToDouble(ApplyRegex(newProduct.ShippingPrice.ToString(), selector.Pattern, selector.Replacement));
                            break;
                        case SelectorType.ShippingCurrency:
                            newProduct.ShippingCurrency = ApplyRegex(newProduct.ShippingCurrency, selector.Pattern, selector.Replacement);
                            break;
                        case SelectorType.Stock:
                            newProduct.Stock = ApplyRegex(newProduct.Stock.ToString(), selector.Pattern, selector.Replacement);
                            break;
                            // agregar más casos para cada tipo de selector que quieras iterar
                    }
                }
            }
        }

        newProduct.StoreId = partnerStore.Id;
        newProduct.Status = "In stock";
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
