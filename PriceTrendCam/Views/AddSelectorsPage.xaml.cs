using Microsoft.UI.Xaml.Controls;

using PriceTrendCam.ViewModels;
using Windows.ApplicationModel;

namespace PriceTrendCam.Views;

// To learn more about WebView2, see https://docs.microsoft.com/microsoft-edge/webview2/.
public sealed partial class AddSelectorsPage : Page
{
    public AddSelectorsViewModel ViewModel
    {
        get;
    }

    public AddSelectorsPage()
    {
        ViewModel = App.GetService<AddSelectorsViewModel>();
        InitializeComponent();

        ViewModel.WebViewService.Initialize(WebView);
    }

    private void SaveButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }

    private void DataPreviewButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }

    private void ElementPreviewButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }

    private void SelectButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }

    private void CancelButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }
    private void WebView_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _ = OnPointerPressed(sender, e);
    }
    private async Task OnPointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        // Obtiene las coordenadas del click
        int xCoord = (int)e.GetCurrentPoint(WebView).Position.X;
        int yCoord = (int)e.GetCurrentPoint(WebView).Position.Y;

        // Ruta del archivo JavaScript
        string getSelectorScriptPath = Path.Combine(Package.Current.InstalledLocation.Path, "Scripts", "getSelector.js");

        // Lee el contenido del archivo JavaScript
        string getSelectorScriptContent = File.ReadAllText(getSelectorScriptPath);

        // Crea la parte adicional del script que llama a la función "getCssSelector"
        string getCssSelectorScriptPart = @"getCssSelector(document.elementFromPoint(" + xCoord + ", " + yCoord + "));";

        // Concatena la parte adicional del script con el contenido original
        getSelectorScriptContent += getCssSelectorScriptPart;

        // Ejecuta el script que obtiene el selector CSS del elemento
        string cssSelector = await WebView.ExecuteScriptAsync(getSelectorScriptContent);

        // Actualiza el cuadro de texto con el selector CSS
        SelectorTextBox.Text = cssSelector;

        // Crea el script que se encarga de resaltar el elemento en la página
        string getCssSelectorScriptPart2 = @"addMarginToSelector(" + cssSelector + ");";
        getSelectorScriptContent += getCssSelectorScriptPart2;
        await WebView.ExecuteScriptAsync(getSelectorScriptContent);
    }
}
