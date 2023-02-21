using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HtmlAgilityPack;
using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.Core.Helpers;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Core.Services;

namespace PriceTrendCam.ViewModels;
public class MainViewModel : ObservableRecipient
{
    public MainViewModel()
    {
        AdvancedSearchCommand = new AsyncRelayCommand(AdvancedSearchAsync);
    }
    public IAsyncRelayCommand AdvancedSearchCommand
    {
        get;
    }
    private string textBoxSearch;
    public string TextBoxSearch
    {
        get => textBoxSearch;
        set => SetProperty(ref textBoxSearch, value);
    }
    private async Task AdvancedSearchAsync()
    {
        if (await Url.IsValid(textBoxSearch))
        {
            //si la url es valida se pasa al siguiente paso que es buscar si la url tiene un sitemap y selctores asignados
            List<ProductInfo> Products = await App.PriceTrackerService.GetAllWithChildrenAsync<ProductInfo>();
            //var query = Products.Where(s => s.Url.Equals(textBoxSearch))?.ToList();

            //var IsRegistered = ((Products?.Where(s => s.Url.Equals(textBoxSearch))?.ToList().Count) > 0) ? true : false;
            var IsRegistered = ((Products?.Where(s => s?.Url?.Equals(textBoxSearch) ?? false)?.ToList().Count ?? 0) > 0);

            var Stores = await App.PriceTrackerService.GetAllWithChildrenAsync<Store>();
            var UrlShop = Stores.SelectMany(s => s.Urls).Where(u => textBoxSearch.Contains(u.Url)).ToList();

            if (UrlShop.Count == 0 || UrlShop == null)
            {
                Console.Write("No selectors assigned to Store");
                return;
            }
            // sino se pasa al siguiente caso de decir esta url no tiene sitemaps ni selectores asignados
            if (IsRegistered)
            {
                Console.Write("The product is registered");
                return;
            }
            var id_sitemap = UrlShop.First().Id;
            // en este caso obtenemos la url del producto y guardamos respecto a su url

            var node = await HtmlDocumentService.LoadPageAsync(textBoxSearch);

            var MetaTitle = HtmlDocumentService.GetMetaTitle(node);
            var MetaDescription = HtmlDocumentService.GetMetaDescription(node);
            var MetaImage = HtmlDocumentService.GetMetaImage(node);

            //descargar la meta imagen o obtener de internet?
            var producto1 = new ProductInfo()
            {
                Name = MetaTitle,
                Description = MetaDescription,
                Image = MetaImage,
                Date = DateTime.Now
            };

            await App.PriceTrackerService.InsertAsync<ProductInfo>(producto1);
        }
        //si la url no es valida eso quiere decir que se ha ingresado un termino de busqueda
        //lo siguiente es utilizar los diccionarios ParseWebSiteJsonLdForSearchAction para obtener una url de busqueda y obtener el buscador de la web
        
        // si no hay ningun sitemap informar al usuario de como realizar este proceso
        var data = await App.PriceTrackerService.GetAllAsync<ProductInfo>();
    }
}
