using System.Text.RegularExpressions;
using HtmlAgilityPack;
using PriceTrendCam.Core.Models;

namespace PriceTrendCam.Core.Services;
#nullable enable
public class ParseInfoService
{
    private static string? ApplyRegex(string value, string pattern, string replace)
    {
        if (string.IsNullOrEmpty(pattern)) return null;

        var regex = new Regex(pattern);
        value = regex.Replace(value, replace);

        return value;
    }
    private static string? GetValue(HtmlNode document, string cssSelector, string attribute)
    {
        return HtmlDocumentService.GetMetaValue(document, cssSelector, attribute);
    }
    public static async Task<ProductInfo> GetProductFromUrl(string url, Store partnerStore, List<Selector?> selectorsFromStore)
    {
        var newProduct = new ProductInfo();
        var document = await HtmlDocumentService.LoadPageAsync(url);

        // Crear variables para cada atributo de newProduct
        string? name, description, image, priceString, priceCurrency, shippingPriceString, shippingCurrency, stockString;
        name = description = image = priceString = priceCurrency = shippingPriceString = shippingCurrency = stockString = null;

        foreach (var selector in selectorsFromStore)
        {
            if (Enum.TryParse(selector.Type, out SelectorType selectorTypeEnum))
            {
                var listPattern = selector.RegexMethods.Select(e => e.Pattern).FirstOrDefault();
                var listReplacement = selector.RegexMethods.Select(e => e.Replacement).FirstOrDefault();

                var value = GetValue(document, selector.CssSelector, selector.Attribute);

                if (value == null) continue;

                value = ApplyRegex(value, listPattern, listReplacement);

                switch (selectorTypeEnum)
                {
                    case SelectorType.Name:
                        name = value;
                        break;
                    case SelectorType.Description:
                        description = value;
                        break;
                    case SelectorType.Image:
                        image = value;
                        break;
                    case SelectorType.Price:
                        priceString = value;
                        break;
                    case SelectorType.PriceCurrency:
                        priceCurrency = value;
                        break;
                    case SelectorType.Shipping:
                        shippingPriceString = value;
                        break;
                    case SelectorType.ShippingCurrency:
                        shippingCurrency = value;
                        break;
                    case SelectorType.Stock:
                        stockString = value;
                        break;
                    // agregar más casos para cada tipo de selector que quieras iterar
                    default:
                        // manejo del caso predeterminado (si corresponde)
                        break;
                }
            }
        }

        newProduct.Price = ConvertValue(priceString, (double?)null);
        newProduct.ShippingPrice = ConvertValue(shippingPriceString, (double?)null);
        newProduct.Stock = ConvertValue(stockString, (double?)null);

        newProduct.Name = name;
        newProduct.Description = description;
        newProduct.Image = image;
        newProduct.PriceCurrency = priceCurrency;
        newProduct.ShippingCurrency = shippingCurrency;

        newProduct.Status = newProduct.Stock <= 0 ? ProductStatus.OutOfStock : ProductStatus.Active;

        newProduct.StoreId = partnerStore.Id;
        newProduct.Url = url;
        newProduct.StoreName = partnerStore.Name;
        newProduct.Date = DateTime.UtcNow;
        newProduct.PriceCurrency = "MXN";
        newProduct.ShippingCurrency = "MXN";

        return newProduct;
    }
    private static T ConvertValue<T>(string value, T defaultValue = default)
    {
        if (string.IsNullOrEmpty(value)) return defaultValue;

        if (typeof(T) == typeof(double?) && double.TryParse(value, out var doubleValue))
        {
            return (T)(object)doubleValue;
        }
        // Puedes agregar más tipos de conversión aquí
        return defaultValue;
    }
}
