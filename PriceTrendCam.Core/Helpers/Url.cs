using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using PriceTrendCam.Core.Services;

namespace PriceTrendCam.Core.Helpers;
public class Url
{
    public static async Task<bool> IsValid(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return false;
        }

        url = await NormalizeUrl(url);

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            // Intenta agregar el esquema HTTP si falta
            if (Uri.TryCreate("http://" + url, UriKind.Absolute, out uri))
            {
                url = uri.AbsoluteUri;
            }
            else
            {
                Debug.WriteLine($"La URL no es válida: {url}");
                return false;
            }
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            Debug.WriteLine($"El esquema de la URL no es HTTP o HTTPS: {url}");
            return false;
        }

        try
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

                using (var response = await client.GetAsync(uri))
                {
                    Debug.WriteLine($"Código de estado de la respuesta: {response.StatusCode}");

                    if (response.StatusCode == HttpStatusCode.OK ||
                        response.StatusCode == HttpStatusCode.MovedPermanently)
                    {
                        return true;
                    }

                    if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        return false;
                    }
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"Se produjo una excepción al enviar la solicitud HTTP a {url}: {ex.Message}");
        }

        return false;
    }
    public static bool TryCreateUri(String potentialUri, out Uri result)
    {
        Uri uri;
        if ((Uri.TryCreate(potentialUri, UriKind.Absolute, out uri) || Uri.TryCreate("http://" + potentialUri, UriKind.Absolute, out uri)) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            result = uri;
            return true;
        }
        else
        {
            result = null;
            return false;
        }
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
    public static async Task<string> GetRedirectUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return null;
        }

        url = await NormalizeUrl(url);

        // Comprobar si la URL es válida según la sintaxis de una URL
        var regex = new Regex(@"^(https?://)?([\w-]+\.)+[\w-]+(/[^\s]*)?$");

        if (!regex.IsMatch(url))
        {
            Debug.WriteLine($"La URL no es válida según la expresión regular: {url}");
            return null;
        }

        // 1. Crear un cliente HTTP
        using (var client = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false }))
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

            // 2. Enviar una solicitud HTTP GET a la URL original
            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(url);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Se produjo una excepción al enviar la solicitud HTTP a {url}: {ex.Message}");
                return null;
            }

            // 3. Verificar el código de estado de la respuesta
            if (response.StatusCode == HttpStatusCode.Redirect ||
                response.StatusCode == HttpStatusCode.MovedPermanently)
            {
                // 4. Obtener la URL a la que se redirecciona
                var locationHeader = response.Headers.Location?.ToString();

                if (!string.IsNullOrEmpty(locationHeader))
                {
                    // 5. Modificar manualmente la URL final con esquema "https" y eliminar el puerto
                    var finalUri = new UriBuilder(locationHeader);
                    finalUri.Scheme = "https";
                    finalUri.Port = -1;

                    return finalUri.Uri.AbsoluteUri;
                }
            }
        }

        // 6. Si no se produjo una redirección, devolver la URL original
        return url;
    }


}