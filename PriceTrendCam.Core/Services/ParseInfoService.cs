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
        var values = new Dictionary<string, string>();

        foreach (var selector in selectorsFromStore)
        {
            if (Enum.TryParse(selector.Type, out SelectorType selectorTypeEnum))
            {
                var listPattern = selector.RegexMethods.Select(e => e.Pattern).FirstOrDefault();
                var listReplacement = selector.RegexMethods.Select(e => e.Replacement).FirstOrDefault();

                var value = GetValue(document, selector.CssSelector, selector.Attribute);

                if (value == null) continue;

                value = ApplyRegex(value, listPattern, listReplacement);

                values[selector.Type] = value;
            }
        }

        newProduct.Price = ConvertValue<double?>(values.GetValueOrDefault("Price")?.ToString(), null);
        newProduct.ShippingPrice = ConvertValue<double?>(values.GetValueOrDefault("Shipping")?.ToString(), null);
        newProduct.Stock = ConvertValue<double?>(values.GetValueOrDefault("Stock")?.ToString(), null);

        newProduct.Name = values.GetValueOrDefault("Name")?.ToString();
        newProduct.Description = values.GetValueOrDefault("Description")?.ToString();
        newProduct.Image = values.GetValueOrDefault("Image")?.ToString();
        newProduct.PriceCurrency = values.GetValueOrDefault("PriceCurrency")?.ToString();
        newProduct.ShippingCurrency = values.GetValueOrDefault("ShippingCurrency")?.ToString();

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
