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
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

            var response = await client.GetAsync(url);

            // Imprimir el código de estado de la respuesta
            Debug.WriteLine($"Código de estado de la respuesta: {response.StatusCode}");

            // Comprobar si la respuesta tiene un código de estado válido
            if (response.StatusCode == HttpStatusCode.OK) { return true; }
            if (response.StatusCode == HttpStatusCode.MovedPermanently) { return true; }
            if (response.StatusCode == HttpStatusCode.Forbidden) { return false; }
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
    public static async Task<bool> HasRedirectToWww(string url)
    {
        // 1. Crear una instancia de HttpClient con HttpClientHandler configurado para no seguir automáticamente las redirecciones.
        var httpClient = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false });

        // 2. Configurar el encabezado User-Agent de la solicitud.
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

        // 3. Enviar una solicitud HTTP GET a la URL que se desea verificar.
        var response = await httpClient.GetAsync(url);

        // 4. Obtener la respuesta de la solicitud HTTP y verificar si se produjo una redirección.
        if (response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.MovedPermanently)
        {
            // 5. Si se produjo una redirección, verificar si la URL final en la respuesta contiene el prefijo "www".
            var locationHeader = response.Headers.Location?.ToString();
            if (locationHeader != null && locationHeader.StartsWith("http://www.") || locationHeader.StartsWith("https://www."))
            {
                return true;
            }
        }
        return false;
    }
}