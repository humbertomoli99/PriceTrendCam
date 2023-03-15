using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using PriceTrendCam.Core.Helpers;
using PriceTrendCam.Core.Models;
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
    private string cssSelector;
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
        _elementPreviewModeIsActive = true;
        _selectionModeIsActive = true;
        AttributesComboBox = new ObservableCollection<string>();
        InitializeTypeDataComboBox();

    }
    private void InitializeTypeDataComboBox()
    {
        TypeDataComboBox = new ObservableCollection<string>(Enum.GetNames(typeof(SelectorType)));
    }
    private async Task<List<string>> GetListAttributesAsync(string element)
    {
        var myElement = await ExecuteScriptAsync(@"getCssSelector( " + element + ");");
        var result = await ExecuteScriptAsync(@"getAttributeNames(" + myElement + ");");
        var regex = new Regex("[\"\\[\\]]");
        var cleanResult = regex.Replace(result, "");
        var attributes = cleanResult.Split(',').ToList();

        return attributes;
    }

    private async Task GetAttributes()
    {
        AttributesComboBox.Clear();

        string miCadena = SelectorAutoSuggestBox.Text;

        if (miCadena.EndsWith("."))
        {
            miCadena = miCadena.Substring(0, miCadena.Length - 1);
        }

        var attributes = await GetListAttributesAsync(miCadena);

        // Agregar valor por defecto si no existe
        if (!attributes.Contains("innerText")) AttributesComboBox.Add(SelectorAutoSuggestBox.Text + "innerText");
        if (!attributes.Contains("innerHTML")) AttributesComboBox.Add(SelectorAutoSuggestBox.Text + "innerHTML");
        if (!attributes.Contains("outerHTML")) AttributesComboBox.Add(SelectorAutoSuggestBox.Text + "outerHTML");

        if (attributes[0] != "null")
        {
            foreach (var attribute in attributes)
            {
                if (string.IsNullOrEmpty(attribute)) continue;
                AttributesComboBox.Add(SelectorAutoSuggestBox.Text + attribute);
            }
        }
    }

    private void SaveButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }

    private async void DataPreviewButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        try
        {
            var resultCommand = await ExecuteScriptAsync(SelectorAutoSuggestBox.Text);

            if (!string.IsNullOrWhiteSpace(PatternTextBox.Text))
            {
                var input = await ExecuteScriptAsync(SelectorAutoSuggestBox.Text);
                var pattern = PatternTextBox.Text;
                var replace = ReplacementTextBox.Text;

                var newText = ApplyRegex(input, pattern, replace);
                _messagePreviewSelectorValue = newText;
            }
            else
            {
                _messagePreviewSelectorValue = resultCommand;
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
        catch (Exception ex)
        {
            // Manejo de excepciones
            var dialog = new ContentDialog
            {
                Title = "Error",
                XamlRoot = XamlRoot,
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Close,
                Content = "An error occurred: " + ex.Message
            };

            await dialog.ShowAsync();
        }
    }
    private string ApplyRegex(string input, string pattern, string replace)
    {
        var regex = new Regex(pattern);
        var newText = regex.Replace(input, replace);
        return newText;
    }
    private async void ElementPreviewButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if ((bool)ElementPreviewButton.IsChecked || _showElementPreview)
        {
            await AddSelectorMargin();
        }
        else
        {
            await DeactivateElementPreviewMode();
        }
    }
    private async void SelectButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_selectionModeIsActive)
        {
            await ActivateElementSelectionMode();
        }
        else
        {
            await DeactivateElementSelectionMode();
        }
    }
    private async Task ToggleLinksAndSvg(bool linksEnabled, bool svgEnabled)
    {
        await ExecuteScriptAsync($"toggleLinks({linksEnabled.ToString().ToLower()})");
        await ExecuteScriptAsync($"toggleSvg({svgEnabled.ToString().ToLower()})");
        await ExecuteScriptAsync("isMarginActive = " + svgEnabled.ToString().ToLower() + ";");
    }
    private async Task AddSelectorMargin()
    {
        await ActivateElementPreviewMode();

        if (_selectedCssSelector != "")
        {
            var isElementInDOM = bool.Parse(await ExecuteScriptAsync(@"isElementInDOM('" + _selectedCssSelector + "');"));

            if (isElementInDOM == false)
            {
                await DeactivateElementPreviewMode();
            }
            if (isElementInDOM == true)
            {
                await ExecuteScriptAsync(@"addMarginToSelector('" + _selectedCssSelector + "');");
            }
        }

        if (_selectorsTree == null || _selectorsTree.Count == 0) return;
        await ExecuteScriptAsync(@"addMarginToSelector('" + _selectedCssSelector + "');");
    }
    private async Task ActivateElementPreviewMode()
    {
        _showElementPreview = false;
        _elementPreviewModeIsActive = true;
        ElementPreviewButton.IsChecked = true;

        await ToggleLinksAndSvg(false, true);
    }

    private async Task DeactivateElementPreviewMode()
    {
        _showElementPreview = true;
        _elementPreviewModeIsActive = false;
        ElementPreviewButton.IsChecked = false;

        await ToggleLinksAndSvg(true, false);
    }

    private async Task ActivateElementSelectionMode()
    {
        SelectButton.IsChecked = true;
        _selectionModeIsActive = false;

        ElementPreviewButton.IsChecked = false;
        _elementPreviewModeIsActive = false;

        await ToggleLinksAndSvg(false, true);

        SetButtonsVisibility(true);

        if (_selectorsTree == null || _selectorsTree.Count == 0) return;

        await ExecuteScriptAsync(@"addMarginToSelector('" + _selectorsTree[_activeSelection] + "');");
    }

    private async Task DeactivateElementSelectionMode()
    {
        SelectButton.IsChecked = false;
        _selectionModeIsActive = true;

        await ToggleLinksAndSvg(true, false);

        SetButtonsVisibility(false);
    }
    private void SetButtonsVisibility(bool visible)
    {
        ChildrenButton.Visibility = visible ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        ParentButton.Visibility = visible ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        DoneButton.Visibility = visible ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        TxtSelectedElement.Visibility = visible ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
    }
    private void CancelButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }
    private async void WebView_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _elementPreviewModeIsActive = true;
    }

    private async void WebView_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
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
        cssSelector = await ExecuteScriptAsync(@"getCssSelector(document.elementFromPoint(" + xCoord + ", " + yCoord + "));");

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
    private async void WebView_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        // Ruta del archivo JavaScript
        var scriptFilePath = Path.Combine(Package.Current.InstalledLocation.Path, "Scripts", "getSelector.js");

        // Lee el contenido del archivo JavaScript
        var scriptFile = await StorageFile.GetFileFromPathAsync(scriptFilePath);
        var scriptContent = await FileIO.ReadTextAsync(scriptFile);
        await ExecuteScriptAsync(scriptContent);

        AddressBar.Text = sender.Source.ToString();

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
        await ToggleLinksAndSvg(true, false);
        SetButtonsVisibility(false);

        _selectionModeIsActive = true;

        if (_selectorsTree == null) return;
        if (_selectorsTree.Count == 0) return;

        SelectorAutoSuggestBox.Text = "document.querySelector('" + _selectorsTree[_activeSelection] + "')";
        _selectedCssSelector = _selectorsTree[_activeSelection];
        ViewModel.selectedCssSelector = _selectedCssSelector;


        SelectButton.IsChecked = false;
        ElementPreviewButton.IsChecked = false;
        //deshabilitar el visualizar script y el visualizador
    }

    private void GetTypeDataComboBox_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        GetTypeDataComboBox.SelectedIndex = 0;
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
    private async void AddressBar_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        AddressBar.Text = await Url.GetRedirectUrl(await Url.NormalizeUrl(AddressBar.Text));
        WebView.Source = new Uri(AddressBar.Text);
    }

    private async void SelectorAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            // Obtener el texto actual del AutoSuggestBox
            string currentText = SelectorAutoSuggestBox.Text;

            // Verificar si el último carácter es un punto
            if (currentText.EndsWith("."))
            {
                await GetAttributes();
            }
        }
    }

    private async void GetTypeDataComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBox comboBox = sender as ComboBox;
        if (comboBox == null)
        {
            return;
        }

        string selectedItem = comboBox.SelectedItem as string;
        if (selectedItem == null)
        {
            return;
        }

        var idStore = ViewModel.GetStore.Id;
        var selector = await App.PriceTrackerService.GetAllWithChildrenAsync<Selector>();
        var selectorsFromStore = selector.Where(s => s.StoreId == idStore).ToList();

        ViewModel.GetListSelectors.Clear();

        SelectorType selectedType;
        if (!Enum.TryParse(selectedItem, out selectedType))
        {
            return;
        }

        var storeUrls = selectorsFromStore.Where(s => s.Type == selectedType.ToString()).ToList();

        foreach (var url in storeUrls)
        {
            ViewModel.GetListSelectors.Add(url);
        }
    }

    private async void ObjectSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Obtener el objeto Selector seleccionado
        if (ObjectSelector.SelectedItem is Selector selector)
        {
            // Obtener el valor de CssSelector
            SelectorAutoSuggestBox.Text = selector.Command;
            _selectedCssSelector = selector.CssSelector;
            _showElementPreview = true;
            ElementPreviewButton_Click(sender,e);
        }
    }
}
