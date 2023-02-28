using Microsoft.UI.Xaml.Controls;

using PriceTrendCam.ViewModels;
using Windows.ApplicationModel;
using Windows.Storage;

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
        var point = e.GetCurrentPoint(WebView).Position;
        int xCoord = (int)point.X;
        int yCoord = (int)point.Y;

        // Ruta del archivo JavaScript
        string scriptFilePath = Path.Combine(Package.Current.InstalledLocation.Path, "Scripts", "getSelector.js");

        // Lee el contenido del archivo JavaScript
        StorageFile scriptFile = await StorageFile.GetFileFromPathAsync(scriptFilePath);
        string scriptContent = await FileIO.ReadTextAsync(scriptFile);

        // Crea la parte adicional del script que llama a la función "getCssSelector"
        string getCssSelectorScriptPart = @"getCssSelector(document.elementFromPoint(" + xCoord + ", " + yCoord + "));";

        // Concatena la parte adicional del script con el contenido original
        scriptContent += getCssSelectorScriptPart;

        // Ejecuta el script que obtiene el selector CSS del elemento
        string cssSelector = await WebView.ExecuteScriptAsync(scriptContent);

        // Actualiza el cuadro de texto con el selector CSS
        SelectorTextBox.Text = cssSelector;

        // Crea el script que se encarga de resaltar el elemento en la página
        scriptContent += @"addMarginToSelector(" + cssSelector + ");";
        await WebView.ExecuteScriptAsync(scriptContent);
    }

    private void WebView_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        //_ = OnPointerPressed(sender, e);
    }

    private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBoxItem selectedItem = TypeComboBox.SelectedItem as ComboBoxItem;
        int tagValue = Convert.ToInt32(selectedItem.Tag);

        // Haz algo con el valor de tagValue
        switch (tagValue)
        {
            case 1:
                // Hacer algo cuando se selecciona Text
                RegexTextBox.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                break;
            case 2:
                // Hacer algo cuando se selecciona Link
                RegexTextBox.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                break;
            case 3:
                // Hacer algo cuando se selecciona Popup Link
                RegexTextBox.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                break;
            case 4:
                // Hacer algo cuando se selecciona Image
                RegexTextBox.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                break;
            case 5:
                // Hacer algo cuando se selecciona Table
                RegexTextBox.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                break;
            case 6:
                // Hacer algo cuando se selecciona Element Attribute
                RegexTextBox.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                break;
            case 7:
                // Hacer algo cuando se selecciona HTML
                RegexTextBox.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                break;
            case 8:
                // Hacer algo cuando se selecciona Element
                RegexTextBox.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                break;
            case 9:
                // Hacer algo cuando se selecciona Element Scrolldown
                RegexTextBox.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                break;
            case 10:
                // Hacer algo cuando se selecciona Element Click
                RegexTextBox.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                break;
            case 11:
                // Hacer algo cuando se selecciona Grouped
                RegexTextBox.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                break;
            case 12:
                // Hacer algo cuando se selecciona Sitemap.xml Links
                RegexTextBox.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                break;
            case 13:
                // Hacer algo cuando se selecciona Pagination (Beta)
                RegexTextBox.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                break;
            case 14:
                // Hacer algo cuando se selecciona Console Command
                RegexTextBox.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                break;
            default:
                break;
        }
    }
}
