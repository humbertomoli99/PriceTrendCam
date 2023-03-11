using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using PriceTrendCam.ViewModels;
using Windows.ApplicationModel;
using Windows.Storage;

namespace PriceTrendCam.Views;

// To learn more about WebView2, see https://docs.microsoft.com/microsoft-edge/webview2/.
public sealed partial class AddSelectorsPage : Page
{
    private bool isWebViewReady;
    private List<string> _selectorsTree;
    private int _activeSelection;
    private bool _showElementPreview;
    private string _selectedCssSelector;
    private string _messagePreviewSelectorValue;
    private bool _elementPreviewModeIsActive;
    private bool _selectionModeIsActive;

    public ObservableCollection<string> AttributesComboBox
    {
        get;
        set;
    }
    public ObservableCollection<string> TypeDataComboBox
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
        _activeSelection = 0;
        _showElementPreview = true;
        _selectedCssSelector = string.Empty;
        _messagePreviewSelectorValue = string.Empty;
        _elementPreviewModeIsActive = false;
        _selectionModeIsActive = true;
        AttributesComboBox = new ObservableCollection<string>();
        InitializeTypeDataComboBox();
    }
    private void InitializeTypeDataComboBox()
    {
        TypeDataComboBox = new ObservableCollection<string>
        {
            "Title",
            "Description",
            "Image",
            "Price",
            "Price Currency",
            "Shipping",
            "Shipping Currency",
            "Stock",
        };
    }
    private async Task GetAttributes()
    {
        var result = await ExecuteScriptAsync(@"getAttributeNames('" + _selectedCssSelector + "');");
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

    private void SaveButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }

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
        if (!string.IsNullOrWhiteSpace(RegexTextBox.Text))
        {
            var input = await ExecuteScriptAsync(SelectorTextBox.Text + "." + attribute);
            var pattern = RegexTextBox.Text;
            var regex = new Regex(pattern);

            Match match = regex.Match(input);

            _messagePreviewSelectorValue = attribute + ": " + match.Value;
        }
        else
        {
            _messagePreviewSelectorValue = attribute + ": " + await ExecuteScriptAsync(SelectorTextBox.Text + "." + attribute);
        }

        var dialog = new ContentDialog
        {
            Title = "Data Preview",
            XamlRoot = XamlRoot,
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Close,
            Content = _messagePreviewSelectorValue
        };

        await dialog.ShowAsync();
    }

    private async void ElementPreviewButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        SelectButton.IsChecked = false;
        if ((bool)ElementPreviewButton.IsChecked || _showElementPreview)
        {
            _showElementPreview = false;
            _elementPreviewModeIsActive = true;
            await ExecuteScriptAsync(@"toggleLinks(false)");//desactivar los enlaces
            await ExecuteScriptAsync(@"toggleSvg(true)");
            await ExecuteScriptAsync(@"isMarginActive = true;");

            if (_selectorsTree == null) return;
            if (_selectorsTree.Count == 0) return;

            await ExecuteScriptAsync(@"addMarginToSelector('" + _selectedCssSelector + "');");

        }
        else
        {
            _showElementPreview = true;
            await ExecuteScriptAsync(@"toggleLinks(true)");//desactivar los enlaces
            _elementPreviewModeIsActive = false;
            ElementPreviewButton.IsChecked = false;
            await ExecuteScriptAsync(@"toggleSvg(false)");
            await ExecuteScriptAsync(@"isMarginActive = false;");
        }
    }

    private async void SelectButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_selectionModeIsActive)
        {
            SelectButton.IsChecked = true;
            _elementPreviewModeIsActive = false;
            ElementPreviewButton.IsChecked = false;
            ChildrenButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            ParentButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            DoneButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            TxtSelectedElement.Visibility = Microsoft.UI.Xaml.Visibility.Visible;

            // Activar los enlaces
            await ExecuteScriptAsync(@"toggleLinks(false)");
            await ExecuteScriptAsync(@"toggleSvg(true)");
            await ExecuteScriptAsync(@"isMarginActive = true;");
            _selectionModeIsActive = false;

            if (_selectorsTree == null) return;
            if (_selectorsTree.Count == 0) return;

            await ExecuteScriptAsync(@"addMarginToSelector('" + _selectorsTree[_activeSelection] + "');");
        }
        else
        {
            SelectButton.IsChecked = false;
            _selectionModeIsActive = true;
            // Activar los enlaces
            await ExecuteScriptAsync(@"toggleLinks(true)");
            await ExecuteScriptAsync(@"toggleSvg(false)");
            await ExecuteScriptAsync(@"isMarginActive = false;");

            ChildrenButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            ParentButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            DoneButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            TxtSelectedElement.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }
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
        if (_elementPreviewModeIsActive)
        {
            return;
        }
        _selectionModeIsActive = true;
        // Obtiene las coordenadas del click
        var point = e.GetCurrentPoint(WebView).Position;
        var xCoord = (int)point.X;
        var yCoord = (int)point.Y;

        _activeSelection = 0;

        // Ejecuta el script que obtiene el selector CSS del elemento
        var cssSelector = await ExecuteScriptAsync(@"getCssSelector(document.elementFromPoint(" + xCoord + ", " + yCoord + "));");

        // Llamar a la función JavaScript para obtener el árbol de elementos como una cadena JSON
        var json = await ExecuteScriptAsync("obtenerArbolElementos(" + cssSelector + ")");

        // Analizar la cadena JSON en una lista de strings
        _selectorsTree = JsonSerializer.Deserialize<List<string>>(json);

        if (_selectorsTree == null) return;
        if (_selectorsTree.Count == 0) return;

        // Actualiza el cuadro de texto con el selector CSS
        TxtSelectedElement.Text = cssSelector;

        // Crea el script que se encarga de resaltar el elemento en la página
        var firstSelector = _selectorsTree[0];
        await ExecuteScriptAsync(@"addMarginToSelector('" + firstSelector + "');");

    }

    private void WebView_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        //_ = OnPointerPressed(sender, e);
    }
    private async void WebView_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        // Ruta del archivo JavaScript
        var scriptFilePath = Path.Combine(Package.Current.InstalledLocation.Path, "Scripts", "getSelector.js");

        // Lee el contenido del archivo JavaScript
        var scriptFile = await StorageFile.GetFileFromPathAsync(scriptFilePath);
        var scriptContent = await FileIO.ReadTextAsync(scriptFile);
        await ExecuteScriptAsync(scriptContent);
        // Establecer la variable de estado en verdadero
        isWebViewReady = true;
    }
    private async void ParentButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Comprobar si el WebView2 está listo
        if (!isWebViewReady) return;

        if ((_selectorsTree.Count - 1) > _activeSelection)
        {
            _activeSelection += 1;

            TxtSelectedElement.Text = _selectorsTree[_activeSelection];

            // Crea el script que se encarga de resaltar el elemento en la página
            await ExecuteScriptAsync(@"addMarginToSelector('" + _selectorsTree[_activeSelection] + "');");
        }
    }
    private async void ChildrenButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Comprobar si el WebView2 está listo
        if (!isWebViewReady) return;

        if (_selectorsTree.Count == 0 || _selectorsTree == null) return;
        if (_activeSelection > 0)
        {
            _activeSelection -= 1;

            // Crea el script que se encarga de resaltar el elemento en la página

            await ExecuteScriptAsync(@"addMarginToSelector('" + _selectorsTree[_activeSelection] + "');");
            TxtSelectedElement.Text = _selectorsTree[_activeSelection];
        }
    }
    private async void DoneButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ChildrenButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        ParentButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        DoneButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        TxtSelectedElement.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;

        // Activar los enlaces
        await ExecuteScriptAsync(@"toggleLinks(true)");
        await ExecuteScriptAsync(@"toggleSvg(false)");
        await ExecuteScriptAsync(@"isMarginActive = false;");

        _selectionModeIsActive = true;

        if (_selectorsTree == null) return;
        if (_selectorsTree.Count == 0) return;

        SelectorTextBox.Text = "document.querySelector('" + _selectorsTree[_activeSelection] + "')";
        _selectedCssSelector = _selectorsTree[_activeSelection];
        ViewModel.selectedCssSelector = _selectedCssSelector;


        SelectButton.IsChecked = false;
        ElementPreviewButton.IsChecked = false;
        //deshabilitar el visualizar script y el visualizador
        await GetAttributes();
    }

    private async void SelectorTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        await GetAttributes();
    }

    private void GetTypeDataComboBox_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        GetTypeDataComboBox.SelectedIndex = 0;
    }

    private void GetAttributeComboBox_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        GetAttributeComboBox.SelectedIndex = 0;
    }
    private async Task<string> ExecuteScriptAsync(string script)
    {
        return await WebView.CoreWebView2.ExecuteScriptAsync(script);
    }

    private async void WebView_NavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        // Ruta del archivo JavaScript
        var scriptFilePath = Path.Combine(Package.Current.InstalledLocation.Path, "Scripts", "getSelector.js");

        // Lee el contenido del archivo JavaScript
        var scriptFile = await StorageFile.GetFileFromPathAsync(scriptFilePath);
        var scriptContent = await FileIO.ReadTextAsync(scriptFile);
        await ExecuteScriptAsync(scriptContent);
        // Establecer la variable de estado en verdadero
        isWebViewReady = true;
    }
}
