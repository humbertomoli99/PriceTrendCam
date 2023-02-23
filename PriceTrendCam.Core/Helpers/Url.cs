using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using PriceTrendCam.Core.Services;

namespace PriceTrendCam.Core.Helpers;
public class Url
{
    public static async Task<bool> IsValid(string url)
    {
        if (string.IsNullOrEmpty(url)) { return false; }

        url = await NormalizeUrl(url);
        
        // Comprobar si la URL es válida según la sintaxis de una URL
        var regex = new Regex(@"^(https?://)?([\w-]+\.)+[\w-]+(/[^\s]*)?$");

        if (!regex.IsMatch(url))
        {
            Debug.WriteLine($"La URL no es válida según la expresión regular: {url}");
            return false;
        }

        try
        {
            // Enviar una solicitud HTTP GET a la URL
            //var client = new HttpClient();
            //var response = await client.GetAsync(url);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            int statusCode = (int)response.StatusCode;

            // Imprimir el código de estado de la respuesta
            Debug.WriteLine($"Código de estado de la respuesta: {response.StatusCode}");

            // Comprobar si la respuesta tiene un código de estado válido
            if (statusCode == 200) { return true; }
            if (statusCode == 301) { return true; }
            if (statusCode == 403) { return false; }
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"Se produjo una excepción al enviar la solicitud HTTP a {url}: {ex.Message}");
        }

        return false;
    }
    public static string LimpiarUrl(string url)
    {
        var uri = new Uri(url);
        return uri.GetLeftPart(UriPartial.Path);
    }
    public static async Task<string> NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        // Agregar el esquema https si no está especificado en la URL
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        {
            url = "https://" + url;
        }

        // Validar la URL antes de crear el objeto Uri
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return null; // la URL no es válida, retornar null o lanzar una excepción si es necesario
        }

        // Convertir el host a minúsculas
        var host = uri.Host.ToLower();

        // Reconstruir la URL con el host en minúsculas y con el prefijo "www"
        var path = uri.AbsolutePath + uri.Query;
        var normalizedUrl = new UriBuilder(string.Format("{0}://{1}:{2}{3}", uri.Scheme, host, uri.Port, path)).ToString();

        // Eliminar el puerto 80 o 443 al final de la URL
        normalizedUrl = normalizedUrl.Replace(":80", "");
        normalizedUrl = normalizedUrl.Replace(":443", "");

        return normalizedUrl;
    }
}