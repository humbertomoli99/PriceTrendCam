using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Jint;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using PriceTrendCam.Core.Models;

namespace PriceTrendCam.Core.Services;
public class HtmlDocumentService
{
    /// <summary>
    /// Carga una página web en forma asíncrona y devuelve el nodo HTML raíz.
    /// </summary>
    /// <param name="RequestUri">Dirección URL de la página web a cargar.</param>
    /// <returns>Nodo HTML raíz de la página web cargada.</returns>
    public static async Task<HtmlNode> LoadPageAsync(string RequestUri)
    {
        var client = new HttpClient();
        var response = await client.GetAsync(RequestUri);
        var content = response.Content;
        var document = new HtmlDocument();

        document.LoadHtml(await content.ReadAsStringAsync());
        return document.DocumentNode;
    }
    /// <summary>
    /// Obtiene una lista de URLs de un nodo HTML dado.
    /// </summary>
    /// <param name="DocumentNode">Nodo HTML a examinar.</param>
    /// <returns>Lista de URLs encontrados en el nodo HTML.</returns>
    public static List<string> GetUrls(HtmlNode DocumentNode)
    {
        var listUrl = new List<string>();

        foreach (var node in DocumentNode.QuerySelectorAll("a"))
        {
            listUrl.Add(node.GetAttributeValue("href", ""));
        }
        return listUrl;
    }
    public static async Task<string> GetFaviconUrlAsync(string url)
    {
        var faviconUrl = $"https://{new Uri(url).Host}/favicon.ico";
        var client = new HttpClient();
        var response = await client.GetAsync(faviconUrl);

        if (!response.IsSuccessStatusCode)
        {
            response = await client.GetAsync(url);
            var html = await response.Content.ReadAsStringAsync();

            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(html);

            var linkNode = htmlDoc.DocumentNode.SelectSingleNode("//link[@rel='shortcut icon']");
            if (linkNode != null)
            {
                faviconUrl = linkNode.Attributes["href"].Value;
            }
            else
            {
                return "";
            }
        }

        return faviconUrl;
    }

    /// <summary>
    /// Descarga el favicon de una URL específica.
    /// </summary>
    /// <param name="url">La URL de la página para descargar el favicon.</param>
    /// <param name="folderPath">La Carpeta donde se va a guardar el favicon.</param>
    public static async Task DownloadFaviconAsync(string url, string folderPath)
    {
        try
        {
            var faviconUrl = await GetFaviconUrlAsync(url);

            if (faviconUrl == "")
            {
                Console.WriteLine("No se encontró ningún favicon en la página");
                return;
            }

            var format = GetFaviconFormat(faviconUrl);

            if (format == "")
            {
                Console.WriteLine("Formato de favicon no soportado");
                return;
            }

            var client = new HttpClient();
            var response = await client.GetAsync(faviconUrl);
            var favicon = await response.Content.ReadAsByteArrayAsync();

            var websiteUrl = $"https://{new Uri(url).Host}";
            var fileName = websiteUrl.Replace("https://", "").Replace("www.", "").Replace(".", "_") + $"_favicon.{format}";

            var filePath = Path.Combine(folderPath, fileName);

            File.WriteAllBytes(filePath, favicon);
            Debug.WriteLine("Favicon descargado en: " + filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    private static string GetFaviconFormat(string faviconUrl)
    {
        // Obtener la extensión del archivo del favicon
        var extension = Path.GetExtension(faviconUrl).ToLower().TrimStart('.');

        // Comprobar si el formato es soportado
        if (new List<string> { "ico", "png", "svg", "gif", "jpg" }.Contains(extension))
        {
            return extension;
        }
        else
        {
            // Si el formato no es soportado, lanzar una excepción
            throw new Exception("Formato de favicon no soportado");
        }
    }
    /// Obtiene el valor de un atributo de un nodo HTML específico.
    /// </summary>
    /// <param name="documentNode">El nodo raíz del documento HTML.</param>
    /// <param name="selector">El selector CSS utilizado para seleccionar el nodo deseado.</param>
    /// <param name="attributeName">El nombre del atributo que se desea obtener.</param>
    /// <returns>El valor del atributo o una cadena vacía en caso de no existir.</returns>
    public static string GetMetaValue(HtmlNode DocumentNode, string selector, string attributeName)
    {
        // Selecciona el nodo específico utilizando el selector CSS
        var metaNode = DocumentNode.QuerySelector(selector);

        // Inicializa una variable para almacenar el resultado
        string result;

        // Si se quiere obtener el contenido HTML interno del nodo
        if (attributeName == "innerHTML")
        {
            // Asigna el contenido interno del nodo al resultado
            result = metaNode?.InnerHtml;
        }
        else
        {
            // Asigna el valor del atributo al resultado
            result = metaNode?.GetAttributeValue(attributeName, string.Empty);
        }

        // Devuelve el resultado o una cadena vacía en caso de ser nulo
        return result ?? string.Empty;
    }

    public static string GetMetaTitle(HtmlNode DocumentNode)
    {
        return GetMetaValue(DocumentNode, "head > title", "innerHTML");
    }

    public static string GetMetaDescription(HtmlNode DocumentNode)
    {
        return GetMetaValue(DocumentNode, "head > meta[name='description']", "content");
    }

    public static string GetMetaImage(HtmlNode DocumentNode)
    {
        return GetMetaValue(DocumentNode, "head > meta[property='og:image']", "content");
    }

    public static async Task<List<string>> RemoveDuplicateUrls(string url)
    {
        //Carga la página HTML y la asigna a un objeto HtmlNode
        var htmlNode = await LoadPageAsync(url);

        //Obtiene todas las URLs encontradas en la página HTML
        var allUrls = GetUrls(htmlNode);

        //Elimina duplicados de la lista de URLs
        allUrls = allUrls.Distinct().ToList();

        //Retorna la lista de URLs sin duplicados
        return allUrls;
    }
    public static async Task<string> ExecuteJavaScriptAsync(HtmlNode documentNode, string script)
    {
        // Obtener todos los scripts de la página web
        var scripts = documentNode.Descendants("script")
                                  .Select(s => s.InnerText)
                                  .ToList();

        // Agregar el script pasado como parámetro
        scripts.Add(script);

        // Crear una instancia de JintJsEngine y ejecutar los scripts
        using (var engine = new JintJsEngine())
        {
            foreach (var s in scripts)
            {
                engine.Execute(s);
            }

            // Ejecutar el código JavaScript pasado como parámetro en un hilo separado
            var task = Task.Run(() => engine.Evaluate<string>(script));

            // Esperar el resultado y devolverlo
            return await task;
        }
    }
    /// <summary>
    /// Parses the JSON-LD fragment of a website to obtain search action data.
    /// </summary>
    /// <param name="documentNode">The HTML node of the document to parse.</param>
    /// <returns>A <see cref="SearchActionData"/> object with the parsed data.</returns>
    public static Task<SearchActionData> ParseWebSiteJsonLdForSearchAction(HtmlNode DocumentNode)
    {
        SearchActionData searchActionData = new SearchActionData();
        // Buscar el fragmento de código JSON-LD con la información del sitio web
        var scriptTag = DocumentNode.Descendants("script").FirstOrDefault(
            script => script.GetAttributeValue("type", "") == "application/ld+json");
        if (scriptTag != null)
        {
            // Parsear el contenido JSON-LD
            JObject json = JObject.Parse(scriptTag.InnerHtml);
            if (json.GetValue("@type").ToString() == "WebSite")
            {
                // Obtener el nombre y la URL del sitio web
                searchActionData.Name = json.GetValue("name").ToString();
                searchActionData.WebsiteUrl = json.GetValue("url").ToString();

                // Buscar la acción de búsqueda y obtener la URL y el parámetro de consulta
                JToken action = json.GetValue("potentialAction");
                if (action != null && action["query-input"] != null && action["@type"].ToString() == "SearchAction")
                {
                    searchActionData.SearchUrl = action["target"].ToString();
                    searchActionData.QueryParam = action["query-input"].ToString();
                }
            }
        }
        return Task.FromResult(searchActionData);
    }

}
