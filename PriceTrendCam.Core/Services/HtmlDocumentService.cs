using System.Diagnostics;
using System.Net;
using AngleSharp.Dom;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using JavaScriptEngineSwitcher.Jint;
using Newtonsoft.Json.Linq;
using PriceTrendCam.Core.Helpers;
using PriceTrendCam.Core.Models;

namespace PriceTrendCam.Core.Services;
public class HtmlDocumentService
{
    public static HttpClientHandler CreateHttpClientHandler(bool useCookies, DecompressionMethods decompressionMethods, Dictionary<string, (string cookieName, string cookieValue)> cookies = null)
    {
        var handler = new HttpClientHandler();
        handler.UseCookies = useCookies;

        if (cookies != null && cookies.Count > 0)
        {
            foreach (var cookie in cookies)
            {
                var cookieUrl = cookie.Key;
                var cookieName = cookie.Value.cookieName;
                var cookieValue = cookie.Value.cookieValue;

                handler.CookieContainer.Add(new Uri(cookieUrl), new Cookie(cookieName, cookieValue));
            }
        }

        handler.AutomaticDecompression = decompressionMethods;

        return handler;
    }

    public static HttpClient CreateHttpClient(HttpClientHandler handler, Dictionary<string, string> defaultHeaders = null)
    {
        var client = new HttpClient(handler);

        if (defaultHeaders != null && defaultHeaders.Count > 0)
        {
            foreach (var header in defaultHeaders)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        return client;
    }
    public static async Task<HttpResponseMessage> SendRequestAsync(HttpClient client, string url, HttpMethod httpMethod, Dictionary<string, string> parameters = null)
    {
        // Crear una instancia de HttpContent con los parámetros
        HttpContent content = null;
        if (parameters != null)
        {
            content = new FormUrlEncodedContent(parameters);
        }

        // Crear una instancia de HttpRequestMessage con el método HTTP y URL
        var request = new HttpRequestMessage(httpMethod, url);
        request.Content = content;

        // Enviar la solicitud HTTP
        var response = await client.SendAsync(request);

        return response;
    }

    /// <summary>
    /// Carga una página web en forma asíncrona y devuelve el nodo HTML raíz.
    /// </summary>
    /// <param name="RequestUri">Dirección URL de la página web a cargar.</param>
    /// <returns>Nodo HTML raíz de la página web cargada.</returns>
    public static async Task<HtmlNode> LoadPageAsync(string RequestUri)
    {
        var cookies = new Dictionary<string, (string cookieName, string cookieValue)>
        {
            { RequestUri, ("cookie1", "value1") },
            // Agrega más cookies según tus necesidades
        };

        // Crear una instancia de HttpClientHandler con las configuraciones deseadas
        var handler = CreateHttpClientHandler(true, DecompressionMethods.GZip, cookies);


        // Crear una instancia de HttpClient utilizando el HttpClientHandler y el User Agent
        var defaultHeaders = new Dictionary<string, string>
        {
            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3" },
            { "Accept-Language", "es-ES" },
            // Agrega más encabezados según tus necesidades
        };

        var httpClient = CreateHttpClient(handler, defaultHeaders);

        Dictionary<string, string> parameters = new Dictionary<string, string>
        {
            { "param1", "value1" },
            { "param2", "value2" }
        };

        // Realizar una solicitud HTTP GET
        var response = await SendRequestAsync(httpClient, RequestUri, HttpMethod.Get, parameters);

        // Leer el contenido de la respuesta
        var htmlContent = await response.Content.ReadAsStringAsync();

        // Crear una instancia de HtmlDocument utilizando HtmlAgilityPack
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);

        return htmlDoc.DocumentNode;
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
    public static async Task<string> GetFaviconUrlAsync(string RequestUri)
    {
        var hostUrl = await Url.NormalizeUrl(RequestUri);
        var client = new HttpClient();

        try
        {
            var faviconUrl = $"https://{new Uri(RequestUri).Host}/favicon.ico";
            var response = await client.GetAsync(faviconUrl);

            if (response.IsSuccessStatusCode)
            {
                return faviconUrl;
            }
        }
        catch
        {
            // Omitimos el error y seguimos
        }

        var htmlNode = await LoadPageAsync(RequestUri);
        var linkNode = htmlNode.QuerySelector("link[rel='icon'], link[rel='shortcut icon']");
        var href = linkNode?.GetAttributeValue("href", null);

        if (href == null)
        {
            return "";
        }

        return hostUrl + href;
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
    public static string? GetMetaValue(HtmlNode DocumentNode, string selector, string attributeName)
    {
        // Selecciona el nodo específico utilizando el selector CSS
        selector = selector.Replace("\"", "\'");
        var metaNode = DocumentNode?.QuerySelector(selector);

        // Inicializa una variable para almacenar el resultado
        string result;

        // Si se quiere obtener el contenido HTML interno del nodo
        if (attributeName == "innerHTML")
        {
            // Asigna el contenido interno del nodo al resultado
            result = metaNode?.InnerHtml;
        }
        else if (attributeName == "innerText")
        {
            // Asigna el contenido interno del nodo al resultado
            result = metaNode?.InnerText;
        }
        else if (attributeName == "outerHTML")
        {
            // Asigna el contenido interno del nodo al resultado
            result = metaNode?.OuterHtml;
        }
        else
        {
            // Asigna el valor del atributo al resultado
            result = metaNode?.GetAttributeValue(attributeName, string.Empty);
        }

        // Devuelve el resultado o una cadena vacía en caso de ser nulo
        return result;
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
    public static async Task<SearchActionData> ParseWebSiteJsonLdForSearchAction(HtmlNode documentNode)
    {
        var searchActionData = new SearchActionData();

        // Obtener el nombre y la URL del sitio web
        var nameScript = "document.querySelector('meta[property=\"og:site_name\"]').content";
        var urlScript = "document.querySelector('meta[property=\"og:url\"]').content";
        searchActionData.Name = await ExecuteJavaScriptAsync(documentNode, nameScript);
        searchActionData.WebsiteUrl = await ExecuteJavaScriptAsync(documentNode, urlScript);

        // Obtener la acción de búsqueda y obtener la URL y el parámetro de consulta
        var searchScript = "document.querySelector('script[type=\"application/ld+json\"]').innerText";
        var searchJsonString = await ExecuteJavaScriptAsync(documentNode, searchScript);
        var searchJson = JObject.Parse(searchJsonString);
        if (searchJson.GetValue("@type").ToString() == "WebSite")
        {
            var action = searchJson.SelectToken("$.potentialAction[@type='SearchAction']");
            if (action != null && action["query-input"] != null)
            {
                searchActionData.SearchUrl = action["target"].ToString();
                searchActionData.QueryParam = action["query-input"].ToString();
            }
        }

        return searchActionData;
    }
}
