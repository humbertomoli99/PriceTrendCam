using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HtmlAgilityPack;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.Contracts.Services;
using PriceTrendCam.Core.Helpers;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Core.Services;
using Windows.ApplicationModel.DataTransfer;

namespace PriceTrendCam.ViewModels;
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string textBoxSearch;

    private HtmlNode document;

    private string message;
    private string content;

    public XamlRoot XamlRoot
    {
        get;
        set;
    }

    public bool ClipboardAutomatically
    {
        get;
        set;
    }

    private readonly IClipboardSelectorService _clipboardSelectorService;

    public MainViewModel(IClipboardSelectorService clipboardSelectorService)
    {
        _clipboardSelectorService = clipboardSelectorService;
    }

    public async Task ShowMessageAddProductFromClipboard()
    {
        ClipboardAutomatically = await _clipboardSelectorService.LoadClipboardSettingFromSettingsAsync();

        if (!await IsUrlClipboardValid() && !ClipboardAutomatically) return;

        var urlClipboard = await GetClipboardTextAsync();
        if (await IsRegistered(urlClipboard)) return;

        var urlShop = await GetStoreUrlsByUrl(urlClipboard);
        if (urlShop.Count == 0) return;

        //mensaje de si desea añadir el producto en contrado
        await SearchUrlAsync(await GetClipboardTextAsync());
    }

    public async Task<bool> IsUrlClipboardValid()
    {
        var urlClipboard = await GetClipboardTextAsync();

        if (string.IsNullOrWhiteSpace(urlClipboard))
        {
            return await Url.IsValid(urlClipboard);
        }
        return false;
    }

    public async Task<string> GetClipboardTextAsync()
    {
        var package = Clipboard.GetContent();
        if (package.Contains(StandardDataFormats.Text))
        {
            var text = await package.GetTextAsync();
            return text;
        }
        return string.Empty;
    }

    [RelayCommand]
    public async Task AdvancedSearch()
    {
        try
        {
            if (await Url.IsValid(textBoxSearch))
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

    private string? GetValue(string cssSelector, string attribute)
    {
        return HtmlDocumentService.GetMetaValue(document, cssSelector, attribute);
    }

    private string? ApplyRegex(string value, string pattern, string replacement)
    {
        pattern = Regex.Replace(pattern, @"\""", "");
        replacement ??= string.Empty;
        return Regex.Replace(value, pattern, replacement);
    }

    private async Task<bool> IsRegistered(string url)
    {
        // Buscar si la URL tiene un sitemap y selectores asignados
        var productList = await App.PriceTrackerService.GetAllWithChildrenAsync<ProductInfo>();
        var isRegistered = ((productList?.Where(s => s?.Url?.Equals(url) ?? false)?.ToList().Count ?? 0) > 0);
        return isRegistered;
    }
    private async Task<List<StoreUrl>> GetStoreUrlsByUrl(string url)
    {
        var storesWithUrls = await App.PriceTrackerService.GetAllWithChildrenAsync<Store>();
        var matchingUrls = storesWithUrls
            .SelectMany(s => s.Urls)
            .Where(u => url.Contains(u.Url))
            .ToList();
        return matchingUrls;
    }
    private async Task SearchUrlAsync(string url)
    {
        if (await IsRegistered(url))
        {
            message = "The product is registered";
            content = "The product is already registered and will continue to be tracked, don't worry";
            await ShowMessageError();
            return;
        }

        var urlShop = await GetStoreUrlsByUrl(url);

        if (urlShop.Count == 0)
        {
            message = "The url is not registered in Stores";
            content = "The url is not registered in Stores, please assign selectors and add start url";
            await ShowMessageError();
            return;
        }

        var partnerStore = await App.PriceTrackerService.GetWithChildrenAsync<Store>(urlShop.FirstOrDefault().StoreId);

        if (partnerStore.Urls == null || partnerStore.Urls.Count == 0)
        {
            message = "No selectors assigned to Store";
            content = "The url does not have selectors assigned, we recommend you see the tutorial on how to add one";
            await ShowMessageError();
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

                        double priceValue;
                        if (double.TryParse(price, out priceValue))
                        {
                            newProduct.Price = priceValue;
                        }
                        else
                        {
                            newProduct.Price = null;
                        }
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

                        double shippingPriceValue;
                        if (double.TryParse(ShippingPrice, out shippingPriceValue))
                        {
                            newProduct.ShippingPrice = shippingPriceValue;
                        }
                        else
                        {
                            newProduct.ShippingPrice = null;
                        }
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

                        double stockValue;
                        if (double.TryParse(Stock, out stockValue))
                        {
                            newProduct.Stock = stockValue;
                        }
                        else
                        {
                            newProduct.Stock = null;
                        }
                        break;
                    // agregar más casos para cada tipo de selector que quieras iterar
                    default:
                        // manejo del caso predeterminado (si corresponde)
                        break;
                }
            }
        }

        if (newProduct.Stock <= 0)
        {
            newProduct.Status = ProductStatus.OutOfStock;
        }
        else
        {
            newProduct.Status = ProductStatus.Active;
        }
        newProduct.StoreId = partnerStore.Id;
        newProduct.Url = url;
        newProduct.StoreName = partnerStore.Name;
        newProduct.Date = DateTime.UtcNow;
        //newProduct.Image = GetValue("head > title", "innerHTML");
        newProduct.PriceCurrency = "MXN";
        newProduct.ShippingCurrency = "MXN";

        var isSucces = await App.PriceTrackerService.InsertAsync(newProduct);
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
        await ShowMessageError();
    }

    private async Task SearchTermAsync()
    {
        // Utilizar los diccionarios ParseWebSiteJsonLdForSearchAction para obtener una URL de búsqueda y obtener el buscador de la web
        // Si no hay ningún sitemap, informar al usuario de cómo realizar este proceso
        var productList = await App.PriceTrackerService.GetAllAsync<ProductInfo>();
        // Si no hay buscadores, solo decir al usuario los productos que tiene en seguimiento
    }

    private async Task ShowMessageError()
    {
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
}
