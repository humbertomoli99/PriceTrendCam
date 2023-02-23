using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;

namespace PriceTrendCam.Core.Helpers;
public class Url
{
    public static async Task<bool> IsValid(string url)
    {
        if (string.IsNullOrEmpty(url)) { return false; }

        // Comprobar si la URL es válida según la sintaxis de una URL
        //var regex = new Regex(@"^(https?://)?([\w-]+\.)+[\w-]+(/[^\s]*)?$");

        url = await NormalizeUrl(url);
        
        var regex = new Regex(@"^(https?://)?([\w-]+\.)+[\w-]+(/[^\s]*)?$");

        if (!regex.IsMatch(url))
        {
            Debug.WriteLine($"La URL no es válida según la expresión regular: {url}");
            return false;
        }

        try
        {
            using var client = new HttpClient();
            // Enviar una solicitud HTTP GET a la URL
            var response = await client.GetAsync(url);

            // Imprimir el código de estado de la respuesta
            Debug.WriteLine($"Código de estado de la respuesta: {response.StatusCode}");

            // Comprobar si la respuesta tiene un código de estado válido
            if (response.StatusCode == HttpStatusCode.OK) { return true; }
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

        // Crear un objeto Uri con la URL
        var uri = new Uri(url);

        // Convertir el host a minúsculas
        var host = uri.Host.ToLower();

        // Verificar si el prefijo "www" está presente en el host
        if (host.StartsWith("www."))
        {
            // Eliminar el prefijo "www" del host si está presente
            host = host.Substring(4);
        }

        // Reconstruir la URL con el host en minúsculas y sin el prefijo "www"
        var builder = new UriBuilder(uri.Scheme, host, uri.Port == 443 ? -1 : uri.Port, uri.PathAndQuery);
        var normalizedUrl = builder.ToString();

        return normalizedUrl;
    }
}