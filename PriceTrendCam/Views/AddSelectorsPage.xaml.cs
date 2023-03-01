using System.Text.Json;
using Microsoft.UI.Xaml.Controls;

using PriceTrendCam.ViewModels;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.WebUI;

namespace PriceTrendCam.Views;

// To learn more about WebView2, see https://docs.microsoft.com/microsoft-edge/webview2/.
public sealed partial class AddSelectorsPage : Page
{
    private bool isWebViewReady = false;
    private List<string>? selectoresCSSArbol;
    private int posicionDeSelector;
    private bool showElementPreview;

    public AddSelectorsViewModel ViewModel
    {
        get;
    }

    public AddSelectorsPage()
    {
        ViewModel = App.GetService<AddSelectorsViewModel>();
        InitializeComponent();

        ViewModel.WebViewService.Initialize(WebView);
        showElementPreview = true;
        TypeComboBox.SelectedIndex = 0;
    }

    private void SaveButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }

    private async void DataPreviewButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (!(TypeComboBox.SelectedItem is ComboBoxItem selectedItem))
        {
            return;
        }

        int tagValue = Convert.ToInt32(selectedItem.Tag);

        var TextInner = await WebView.CoreWebView2.ExecuteScriptAsync(@"getElementInnerText('" + selectoresCSSArbol[posicionDeSelector] + "');");
        var TextHref = await WebView.CoreWebView2.ExecuteScriptAsync(@"getLinkHref('" + selectoresCSSArbol[posicionDeSelector] + "');");
        var TextSrc = await WebView.CoreWebView2.ExecuteScriptAsync(@"getElementSrc('" + selectoresCSSArbol[posicionDeSelector] + "');");

        string cssSelector = string.Empty;
        switch (tagValue)
        {
            case 1:
                cssSelector = TextInner;
                break;
            case 2:
                cssSelector = TextInner + "\n" + "Href: " + TextHref;
                break;
            case 4:
                cssSelector = TextInner + "\n" + "Src: " + TextSrc;
                break;
            default:
                return;
        }

        var dialog = new ContentDialog
        {
            Title = "Data Preview",
            XamlRoot = this.XamlRoot,
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Close,
            Content = cssSelector
        };

        await dialog.ShowAsync();
    }

    private async void ElementPreviewButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (showElementPreview)
        {
            await WebView.CoreWebView2.ExecuteScriptAsync(@"toggleSvg(true)");
            await WebView.CoreWebView2.ExecuteScriptAsync(@"isMarginActive = true;");
            await WebView.CoreWebView2.ExecuteScriptAsync(@"addMarginToSelector('" + selectoresCSSArbol[posicionDeSelector] + "');");
            showElementPreview = false;
        }
        else
        {
            await WebView.CoreWebView2.ExecuteScriptAsync(@"toggleSvg(false)");
            await WebView.CoreWebView2.ExecuteScriptAsync(@"isMarginActive = false;");
            showElementPreview = true;
        }
    }

    private async void SelectButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ChildrenButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        ParentButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        DoneButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        TxtSelectedElement.Visibility = Microsoft.UI.Xaml.Visibility.Visible;

        // Activar los enlaces
        await WebView.CoreWebView2.ExecuteScriptAsync(@"toggleLinks(false)");
        await WebView.CoreWebView2.ExecuteScriptAsync(@"toggleSvg(true)");
        await WebView.CoreWebView2.ExecuteScriptAsync(@"isMarginActive = true;");

        if (selectoresCSSArbol == null) return;
        await WebView.CoreWebView2.ExecuteScriptAsync(@"addMarginToSelector('" + selectoresCSSArbol[posicionDeSelector] + "');");
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

        posicionDeSelector = 0;

        // Ejecuta el script que obtiene el selector CSS del elemento
        var cssSelector = await WebView.ExecuteScriptAsync(@"getCssSelector(document.elementFromPoint(" + xCoord + ", " + yCoord + "));");

        // Llamar a la función JavaScript para obtener el árbol de elementos como una cadena JSON
        var json = await WebView.CoreWebView2.ExecuteScriptAsync("obtenerArbolElementos(" + cssSelector + ")");

        // Analizar la cadena JSON en una lista de strings
        selectoresCSSArbol = JsonSerializer.Deserialize<List<string>>(json);
        if (selectoresCSSArbol == null)
        {
            return;
        }
        // Actualiza el cuadro de texto con el selector CSS
        TxtSelectedElement.Text = cssSelector;

        // Crea el script que se encarga de resaltar el elemento en la página
        var firstSelector = selectoresCSSArbol[0];
        await WebView.CoreWebView2.ExecuteScriptAsync(@"addMarginToSelector('" + firstSelector + "');");
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
    private async void WebView_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
    {
        // Ruta del archivo JavaScript
        var scriptFilePath = Path.Combine(Package.Current.InstalledLocation.Path, "Scripts", "getSelector.js");

        // Lee el contenido del archivo JavaScript
        StorageFile scriptFile = await StorageFile.GetFileFromPathAsync(scriptFilePath);
        var scriptContent = await FileIO.ReadTextAsync(scriptFile);
        await WebView.ExecuteScriptAsync(scriptContent);
        // Establecer la variable de estado en verdadero
        isWebViewReady = true;
    }
    private async void ParentButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Comprobar si el WebView2 está listo
        if (!isWebViewReady) return;

        if ((selectoresCSSArbol.Count - 1) > posicionDeSelector)
        {
            posicionDeSelector += 1;

            TxtSelectedElement.Text = selectoresCSSArbol[posicionDeSelector];

            // Crea el script que se encarga de resaltar el elemento en la página
            await WebView.CoreWebView2.ExecuteScriptAsync(@"addMarginToSelector('" + selectoresCSSArbol[posicionDeSelector] + "');");
        }
    }
    private async void ChildrenButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Comprobar si el WebView2 está listo
        if (!isWebViewReady) return;

        if (posicionDeSelector > 0)
        {
            posicionDeSelector -= 1;

            // Crea el script que se encarga de resaltar el elemento en la página
            await WebView.CoreWebView2.ExecuteScriptAsync(@"addMarginToSelector('" + selectoresCSSArbol[posicionDeSelector] + "');");
            TxtSelectedElement.Text = selectoresCSSArbol[posicionDeSelector];
        }
    }
    private async void DoneButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ChildrenButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        ParentButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        DoneButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        TxtSelectedElement.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;

        // Activar los enlaces
        await WebView.CoreWebView2.ExecuteScriptAsync(@"toggleLinks(true)");
        await WebView.CoreWebView2.ExecuteScriptAsync(@"toggleSvg(false)");
        await WebView.CoreWebView2.ExecuteScriptAsync(@"isMarginActive = false;");

        SelectorTextBox.Text = selectoresCSSArbol[posicionDeSelector];
        //deshabilitar el visualizar script y el visualizador
    }
}
