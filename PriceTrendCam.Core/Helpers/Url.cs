using System.Net;
using System.Text.RegularExpressions;

namespace PriceTrendCam.Core.Helpers;
public class Url
{
    public static async Task<bool> IsValid(string url)
    {
        if (string.IsNullOrEmpty(url)) { return false; }

        // Comprobar si la URL es válida según la sintaxis de una URL
        var regex = new Regex(@"^(https?://)?([\w-]+\.)+[\w-]+(/[^\s]*)?$");

        if (!regex.IsMatch(url)) { return false; }

        try
        {
            using var client = new HttpClient();
            // Enviar una solicitud HTTP GET a la URL
            var response = await client.GetAsync(url);

            // Comprobar si la respuesta tiene un código de estado válido
            if (response.StatusCode == HttpStatusCode.OK) { return true; }
        }
        catch (HttpRequestException)
        {
            // Si se produce una excepción al enviar la solicitud HTTP, la URL no es válida
        }

        return false;
    }
}