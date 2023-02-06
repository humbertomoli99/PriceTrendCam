using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace PriceTrendCam.Core.Services;
public class HtmlDocumentService
{
    public static async Task<HtmlNode> LoadPageAsync(string RequestUri)
    {
        var client = new HttpClient();
        var response = await client.GetAsync(RequestUri);
        var content = response.Content;
        var document = new HtmlDocument();

        document.LoadHtml(await content.ReadAsStringAsync());
        return document.DocumentNode;
    }

    public static List<string> GetUrls(HtmlNode htmlUrl)
    {
        var listUrl = new List<string>();

        foreach (var node in htmlUrl.QuerySelectorAll("a"))
        {
            listUrl.Add(node.GetAttributeValue("href", ""));
        }
        return listUrl;
    }
    /// <summary>
    /// Descarga el favicon de una URL específica.
    /// </summary>
    /// <param name="url">La URL de la página para descargar el favicon.</param>
    public static async Task DownloadFaviconAsync(string url)
    {
        try
        {
            // Intenta descargar el favicon directamente a partir de la URL https://www.example.com/favicon.ico
            var faviconUrl = $"https://{new Uri(url).Host}/favicon.ico";
            var client = new HttpClient();
            var response = await client.GetAsync(faviconUrl);

            // Si la descarga directa falla, busca el elemento "link" con "rel=shortcut icon"
            if (!response.IsSuccessStatusCode)
            {
                // Descargar el HTML de la página
                response = await client.GetAsync(url);
                var html = await response.Content.ReadAsStringAsync();

                // Analizar el HTML
                var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(html);

                // Buscar el elemento "link" con "rel=shortcut icon"
                var linkNode = htmlDoc.DocumentNode.SelectSingleNode("//link[@rel='shortcut icon']");
                if (linkNode != null)
                {
                    faviconUrl = linkNode.Attributes["href"].Value;
                }
                else
                {
                    Console.WriteLine("No se encontró ningún favicon en la página");
                    return;
                }
            }

            // Determinar el formato de descarga basándose en la extensión original
            var format = GetFaviconFormat(faviconUrl);

            // Si el formato no es soportado, muestra un error
            if (format == "")
            {
                Console.WriteLine("Formato de favicon no soportado");
                return;
            }

            // Descargar el favicon
            response = await client.GetAsync(faviconUrl);
            var favicon = await response.Content.ReadAsByteArrayAsync();

            // Guardar el favicon en el disco local
            System.IO.File.WriteAllBytes($"favicon.{format}", favicon);
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
}
