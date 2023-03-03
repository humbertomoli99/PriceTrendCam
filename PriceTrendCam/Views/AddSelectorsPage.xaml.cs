using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.ViewModels;
using Windows.ApplicationModel;
using Windows.Storage;

namespace PriceTrendCam.Views;

// To learn more about WebView2, see https://docs.microsoft.com/microsoft-edge/webview2/.
public sealed partial class AddSelectorsPage : Page
{
    private bool isWebViewReady;
    private List<string> _selectorsTree;
    private int _selectorPosition;
    private bool _showElementPreview;
    private string _selectedCssSelector;
    private string _messageSelectorValue;


    public ObservableCollection<string> AttributesComboBox
    {
        get;
        set;
    }
    public AddSelectorsViewModel ViewModel
    {
        get;
    }

    public AddSelectorsPage()
    {
        ViewModel = App.GetService<AddSelectorsViewModel>();
        InitializeComponent();

        ViewModel.WebViewService.Initialize(WebView);
        InitializeVariables();
    }
    private void InitializeVariables()
    {
        isWebViewReady = false;
        _selectorsTree = new List<string>();
        _selectorPosition = 0;
        _showElementPreview = true;
        _selectedCssSelector = string.Empty;
        _messageSelectorValue = string.Empty;
        AttributesComboBox = new ObservableCollection<string>();
        GetAttributeComboBox.SelectedIndex = 0;
        TypeDataComboBox.SelectedIndex = 0;
    }
    private async Task GetAttributes()
    {
        var result = await WebView.CoreWebView2.ExecuteScriptAsync(@"getAttributeNames('" + _selectedCssSelector + "');");
        AttributesComboBox.Clear();

        if (result != null)
    {
            var regex = new Regex("[\"\\[\\]]");
            var cleanResult = regex.Replace(result, "");
            var attributes = cleanResult.Split(',').ToList();

            // Agregar valor por defecto si no existe
            if (!attributes.Contains("innerText")) AttributesComboBox.Add("innerText");
            if (!attributes.Contains("innerHTML")) AttributesComboBox.Add("innerHTML");
            if (!attributes.Contains("outerHTML")) AttributesComboBox.Add("outerHTML");

            foreach (var attribute in attributes)
    {
                AttributesComboBox.Add(attribute);
        }
            GetAttributeComboBox.SelectedIndex = 0;
        }
    }

        int tagValue = Convert.ToInt32(selectedItem.Tag);

        var TextInner = await WebView.CoreWebView2.ExecuteScriptAsync(@"getElementInnerText('" + SelectorTextBox.Text + "');");
        var TextHref = await WebView.CoreWebView2.ExecuteScriptAsync(@"getLinkHref('" + SelectorTextBox.Text + "');");
        var TextSrc = await WebView.CoreWebView2.ExecuteScriptAsync(@"getElementSrc('" + SelectorTextBox.Text + "');");

    private async void DataPreviewButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {

        var attribute = GetAttributeComboBox.Text switch
        {
            "innerText" => "innerText",
            "innerHTML" => "innerHTML",
            "outerHTML" => "outerHTML",
            "class" => "classList",
            "data-linktype" => "dataset.linktype",
            _ => GetAttributeComboBox.Text
        };
        _messageSelectorValue = attribute + ": " + await WebView.CoreWebView2.ExecuteScriptAsync(SelectorTextBox.Text + "." + attribute);
                cssSelector = await WebView.CoreWebView2.ExecuteScriptAsync(SelectorTextBox.Text);
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
            Content = _messageSelectorValue
        };

        await dialog.ShowAsync();
    }

    private async void ElementPreviewButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_showElementPreview)
        {
            _showElementPreview = false;

            await WebView.CoreWebView2.ExecuteScriptAsync(@"toggleSvg(true)");
            await WebView.CoreWebView2.ExecuteScriptAsync(@"isMarginActive = true;");
            if (_selectorsTree == null || _selectorsTree.Count == 0) return;
            await WebView.CoreWebView2.ExecuteScriptAsync(@"addMarginToSelector('" + _selectedCssSelector + "');");
        }
        else
        {
            _showElementPreview = true;

            await WebView.CoreWebView2.ExecuteScriptAsync(@"toggleSvg(false)");
            await WebView.CoreWebView2.ExecuteScriptAsync(@"isMarginActive = false;");
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

        if (_selectorsTree == null || _selectorsTree.Count == 0) return;
        await WebView.CoreWebView2.ExecuteScriptAsync(@"addMarginToSelector('" + _selectorsTree[_selectorPosition] + "');");
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

        _selectorPosition = 0;

        // Ejecuta el script que obtiene el selector CSS del elemento
        var cssSelector = await WebView.ExecuteScriptAsync(@"getCssSelector(document.elementFromPoint(" + xCoord + ", " + yCoord + "));");

        // Llamar a la función JavaScript para obtener el árbol de elementos como una cadena JSON
        var json = await WebView.CoreWebView2.ExecuteScriptAsync("obtenerArbolElementos(" + cssSelector + ")");

        // Analizar la cadena JSON en una lista de strings
        _selectorsTree = JsonSerializer.Deserialize<List<string>>(json);

        if (_selectorsTree == null || _selectorsTree.Count == 0) return;

        // Actualiza el cuadro de texto con el selector CSS
        TxtSelectedElement.Text = cssSelector;

        // Crea el script que se encarga de resaltar el elemento en la página
        var firstSelector = _selectorsTree[0];
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

        if ((_selectorsTree.Count - 1) > _selectorPosition)
        {
            _selectorPosition += 1;

            TxtSelectedElement.Text = _selectorsTree[_selectorPosition];

            // Crea el script que se encarga de resaltar el elemento en la página
            await WebView.CoreWebView2.ExecuteScriptAsync(@"addMarginToSelector('" + _selectorsTree[_selectorPosition] + "');");
        }
    }
    private async void ChildrenButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Comprobar si el WebView2 está listo
        if (!isWebViewReady) return;

        if (_selectorPosition > 0)
        {
            _selectorPosition -= 1;

            // Crea el script que se encarga de resaltar el elemento en la página
            await WebView.CoreWebView2.ExecuteScriptAsync(@"addMarginToSelector('" + _selectorsTree[_selectorPosition] + "');");
            TxtSelectedElement.Text = _selectorsTree[_selectorPosition];
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

        if (_selectorsTree == null || _selectorsTree.Count == 0) return;
        SelectorTextBox.Text = "document.querySelector('" + _selectorsTree[_selectorPosition] + "')";
        _selectedCssSelector = _selectorsTree[_selectorPosition];
        //deshabilitar el visualizar script y el visualizador
        GetAttributeComboBox.SelectedIndex = 0;
        await GetAttributes();
    }
}
