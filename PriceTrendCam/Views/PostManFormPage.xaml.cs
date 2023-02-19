using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using PriceTrendCam.ViewModels;

namespace PriceTrendCam.Views;

public sealed partial class PostManFormPage : Page
{
    public PostManFormViewModel ViewModel
    {
        get;
    }

    public PostManFormPage()
    {
        ViewModel = App.GetService<PostManFormViewModel>();
        InitializeComponent();
    }
    private async void SendButton_Click(object sender, RoutedEventArgs e)
    {
        using var client = new HttpClient();

        var method = GetButton.IsChecked == true ? HttpMethod.Get :
                     PostButton.IsChecked == true ? HttpMethod.Post :
                     PutButton.IsChecked == true ? HttpMethod.Put :
                     DeleteButton.IsChecked == true ? HttpMethod.Delete : HttpMethod.Get;

        var request = new HttpRequestMessage(method, UrlTextBox.Text);
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36 Edge/16.16299");

        if (!string.IsNullOrWhiteSpace(HeadersTextBox.Text))
        {
            var headers = HeadersTextBox.Text.Split('\n');
            foreach (var header in headers)
            {
                var parts = header.Split(':');
                if (parts.Length == 2)
                {
                    request.Headers.Add(parts[0].Trim(), parts[1].Trim());
                }
            }
        }
        if (!string.IsNullOrWhiteSpace(BodyTextBox.Text))
        {
            request.Content = new StringContent(BodyTextBox.Text, Encoding.UTF8, "application/json");
        }

        try
        {
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            ResponseTextBox.Text = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            // Agregue aquí su manejo de excepciones personalizado
            ResponseTextBox.Text = "Error: " + ex.Message;
        }
    }
}
