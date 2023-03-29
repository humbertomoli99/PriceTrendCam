﻿using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Web.WebView2.Core;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Core.Services;
using PriceTrendCam.ViewModels;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using Windows.Storage;

namespace PriceTrendCam.Views;
public class ListItemData
{
    public int Id
    {
        get; set;
    }
    public ICommand Command
    {
        get; set;
    }
    public string CssSelector
    {
        get; set;
    }
    public string Type
    {
        get; set;
    }
    public string Attribute
    {
        get; set;
    }
    public List<RegexMethod> RegexMethods
    {
        get; set;
    }
}
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
    private int textBoxCount;
    private List<RegexMethod> textBoxDataArray;
    private string message;
    private string content;

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
        set;
    }

    readonly ObservableCollection<ListItemData> collection = new();
    public AddSelectorsPage()
    {
        ViewModel = App.GetService<AddSelectorsViewModel>();
        DataContext = ViewModel;
        InitializeComponent();

        ViewModel.WebViewService.Initialize(WebView);
        ViewModel.webview = WebView;
        InitializeVariables();
        AgregarControlesButton_Click(null, null);
        _ = ShowDataInDataPreviewTabAsync();
    }
    private void Eliminar_Click(object sender, RoutedEventArgs e)
    {
        Button? deleteButton = (Button)sender;
        Grid? grid = (Grid)deleteButton.Tag;
        textBoxesStackPanel.Children.Remove(grid);

        textBoxCount--;

        if (textBoxCount == 0)
        {
            foreach (UIElement element in textBoxesStackPanel.Children)
            {
                if (element is Grid)
                {
                    Grid g = (Grid)element;
                    Button button = (Button)g.Children[0];
                    button.IsEnabled = false;
                    break;
                }
            }
        }
    }
    private void AddNewControlsRegex(string regex = "", string replace = "")
    {
        // Crear un nuevo grid para cada TextBox y botón
        Grid newGrid = new Grid();

        ColumnDefinition column1 = new ColumnDefinition();
        column1.Width = new GridLength(1, GridUnitType.Star);
        newGrid.ColumnDefinitions.Add(column1);

        ColumnDefinition column2 = new ColumnDefinition();
        column2.Width = GridLength.Auto;
        newGrid.ColumnDefinitions.Add(column2);

        RowDefinition row1 = new RowDefinition();
        row1.Height = GridLength.Auto;
        newGrid.RowDefinitions.Add(row1);

        RowDefinition row2 = new RowDefinition();
        row2.Height = GridLength.Auto;
        newGrid.RowDefinitions.Add(row2);

        RowDefinition row3 = new RowDefinition();
        row3.Height = GridLength.Auto;
        newGrid.RowDefinitions.Add(row3);

        // Crear un nuevo botón
        Button deleteButton = new Button()
        {
            Content = "🗑",
            Margin = new Thickness(6, 6, 0, 0),
            Tag = newGrid,
            VerticalAlignment = VerticalAlignment.Bottom
        };

        deleteButton.Click += Eliminar_Click;

        // Agregar el botón a la segunda columna del nuevo Grid

        TextBox patternTextBox = new TextBox
        {
            Text = regex,
            Name = "PatternTextBox",
            PlaceholderText = "Pattern",
            Margin = new Thickness(0, 5, 0, 0)
        };

        TextBox replacementTextBox = new TextBox
        {
            Text = replace,
            Name = "ReplacementTextBox",
            PlaceholderText = "Replacement",
            Margin = new Thickness(0, 5, 0, 10)
        };

        Grid.SetColumn(deleteButton, 1);
        Grid.SetRow(patternTextBox, 0);
        Grid.SetRow(replacementTextBox, 1);
        Grid.SetColumnSpan(replacementTextBox, 2);
        // Agregar los controles al Grid contenedor

        newGrid.Children.Add(patternTextBox);
        newGrid.Children.Add(replacementTextBox);
        newGrid.Children.Add(deleteButton);

        textBoxesStackPanel.Children.Add(newGrid);

        textBoxCount++;

        if (textBoxCount == 1)
        {
            //ocultamos el primer boton eliminar
            deleteButton.IsEnabled = false;
            deleteButton.Visibility = Visibility.Collapsed;

            // Crear un nuevo botón Añadir junto al primer textbox
            Button addButton = new Button();
            addButton.VerticalAlignment = VerticalAlignment.Bottom;
            addButton.Margin = new Thickness(6, 0, 0, 0);

            addButton.Content = "➕";
            addButton.Tag = newGrid;
            addButton.Click += AgregarControlesButton_Click;

            Grid.SetColumn(addButton, 2);
            newGrid.Children.Add(addButton);
        }
    }
    private void AgregarControlesButton_Click(object sender, RoutedEventArgs e)
    {
        AddNewControlsRegex();
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
        var cadena2 = $"document.querySelector('{SelectorAutoSuggestBox.Text}')";
        var attributes = await GetListAttributesAsync(cadena2);
        // Agregar valor por defecto si no existe
        if (!attributes.Contains("innerText")) AttributesComboBox.Add("innerText");
        if (!attributes.Contains("innerHTML")) AttributesComboBox.Add("innerHTML");
        if (!attributes.Contains("outerHTML")) AttributesComboBox.Add("outerHTML");

        if (attributes[0] != "null")
        {
            foreach (var attribute in attributes)
            {
                if (string.IsNullOrEmpty(attribute)) continue;
                AttributesComboBox.Add(attribute);
            }
        }
        GetAttributeComboBox.SelectedIndex = 0;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.textBoxDataArray = GetRegexDataList();

        if (ViewModel.isRegistrationSuccessful)
        {
            await ViewModel.GetListSelectorsAsync();
            //TODO: añadir aqui un evento que actualice la lista al momento de guardar los datos
            ElementPreviewButton_Click(sender, e);
            message = "the selector has already added";
            content = $"the selector: \n {cssSelector} \n already added correctly";
        }
        var dialog = new ContentDialog
        {
            Title = message,
            XamlRoot = XamlRoot,
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Close,
            Content = content
        };

        await dialog.ShowAsync();
    }

    private List<RegexMethod> GetRegexDataList()
    {
        List<RegexMethod> textBoxDataList = new List<RegexMethod>();

        foreach (Grid grid in textBoxesStackPanel.Children)
        {
            var pattern = "";
            var Replacement = "";
            foreach (UIElement control in grid.Children)
            {
                if (control is TextBox)
                {
                    TextBox textBox = control as TextBox;

                    if (textBox.Name == "PatternTextBox")
                    {
                        pattern = textBox.Text;
                    }
                    else if (textBox.Name == "ReplacementTextBox")
                    {
                        Replacement = textBox.Text;
                    }
                }
            }

            textBoxDataList.Add(new RegexMethod
            {
                Pattern = pattern,
                Replacement = Replacement
            });
        }
        return textBoxDataList;
    }
    private async Task ShowDataInDataPreviewTabAsync()
    {
        var selectorsList = await App.PriceTrackerService.GetAllWithChildrenAsync<Selector>();
        var selectorsFromStore = selectorsList.Where(s => s.StoreId == ViewModel?.GetStore?.Id).ToList();

        var newProduct = await ParseInfoService.GetProductFromUrl(WebView.Source.ToString(), ViewModel.GetStore, selectorsFromStore);

        PrevTitle.Text = " " + newProduct?.Name;
        PrevDescription.Text = " " + newProduct?.Description;
        PrevPrice.Text = " " + newProduct?.Price?.ToString();
        PrevStatus.Text = " " + newProduct?.Status.ToString();
        PrevShipping.Text = " " + newProduct?.ShippingPrice?.ToString();
        PrevStock.Text = " " + newProduct?.Stock?.ToString();
    }
    private async void DataPreviewButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await ShowDataInDataPreviewTabAsync();
            var scriptElement = $"document.querySelector('{SelectorAutoSuggestBox.Text}').{GetAttributeComboBox.Text}";
            var resultCommand = await ExecuteScriptAsync(scriptElement);

            textBoxDataArray = GetRegexDataList();

            var listSelectors = textBoxDataArray.ToList();

            var listPattern = listSelectors.Select(e => e.Pattern).ToList();
            var listReplacement = listSelectors.Select(e => e.Replacement).ToList();
            
            resultCommand = ApplyRegex(resultCommand, listPattern, listReplacement);

            _messagePreviewSelectorValue = resultCommand;

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

    private string? ApplyRegex(string input, List<string> pattern, List<string> replace)
    {
        if (pattern.Count == 0 || replace.Count == 0) return input;

        if (pattern.Count > 0)
        {
            for (int i = 0; i < pattern.Count; i++)
            {
                //pattern2[i] = Regex.Replace(pattern2[i], @"\""", "");

                var regex = new Regex(pattern[i]);
                input = regex.Replace(input, replace[i]);
            }
        }
        return input;
    }

    private async void ElementPreviewButton_Click(object sender, RoutedEventArgs e)
    {
        if ((bool)ElementPreviewButton.IsChecked || _showElementPreview)
        {
            await AddSelectorMargin();
        }
        else
        {
            await ToggleElementPreviewMode(false);
        }
    }

    private async void SelectButton_Click(object sender, RoutedEventArgs e)
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
        if (string.IsNullOrEmpty(_selectedCssSelector)) return;

        var isElementInDOM = bool.Parse(await ExecuteScriptAsync($"isElementInDOM('{_selectedCssSelector}');"));

        if (!isElementInDOM)
        {
            ElementNotExistTeachingTip.IsOpen = true;
            await ToggleElementPreviewMode(false);
            await Task.Delay(1000); // espera de 1 segundo
            ElementNotExistTeachingTip.IsOpen = false;
            return;
        }

        await ToggleElementPreviewMode(true);
        await ExecuteScriptAsync($"addMarginToSelector('{_selectedCssSelector}');");
    }

    private async Task ToggleElementPreviewMode(bool activatePreviewMode)
    {
        _showElementPreview = !activatePreviewMode;
        _elementPreviewModeIsActive = activatePreviewMode;
        ElementPreviewButton.IsChecked = activatePreviewMode;

        await ToggleLinksAndSvg(!activatePreviewMode, activatePreviewMode);
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
        ChildrenButton.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        ParentButton.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        DoneButton.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        TxtSelectedElement.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
    }
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {

    }
    private async void WebView_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _elementPreviewModeIsActive = true;
    }

    private async void WebView_PointerMoved(object sender, PointerRoutedEventArgs e)
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
        _selectorsTree = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json);

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
    private async void ParentButton_Click(object sender, RoutedEventArgs e)
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
    private async void ChildrenButton_Click(object sender, RoutedEventArgs e)
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
    private async void DoneButton_Click(object sender, RoutedEventArgs e)
    {
        await ToggleLinksAndSvg(true, false);
        SetButtonsVisibility(false);

        _selectionModeIsActive = true;

        if (_selectorsTree == null) return;
        if (_selectorsTree.Count == 0) return;

        SelectorAutoSuggestBox.Text = _selectorsTree[_activeSelection];
        _selectedCssSelector = _selectorsTree[_activeSelection];
        ViewModel.selectedCssSelector = _selectedCssSelector;


        SelectButton.IsChecked = false;
        ElementPreviewButton.IsChecked = false;
        //deshabilitar el visualizar script y el visualizador
        await GetAttributes();
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
        AddressBar.Text = sender.Source.ToString();
        isWebViewReady = true;
        await ShowDataInDataPreviewTabAsync();
    }
    private async void AddressBar_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        AddressBar.Text = await Core.Helpers.Url.GetRedirectUrl(await Core.Helpers.Url.NormalizeUrl(AddressBar.Text));
        WebView.Source = new Uri(AddressBar.Text);
    }

    private async void SelectorAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            // Obtener el texto actual del AutoSuggestBox
            string currentText = SelectorAutoSuggestBox.Text;
            await GetAttributes();
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

        if (ViewModel.GetStore == null) return;

        var idStore = ViewModel.GetStore.Id;

        var selector = await App.PriceTrackerService.GetAllWithChildrenAsync<Selector>();
        if (selector == null) return;

        var selectorsFromStore = selector.Where(s => s.StoreId == idStore).ToList();

        ViewModel.GetListSelectors.Clear();

        SelectorType selectedType;
        if (!Enum.TryParse(selectedItem, out selectedType))
        {
            return;
        }

        var storeUrls = selectorsFromStore.Where(s => s.Type == selectedType.ToString()).ToList();

        await GetListSelectorsAsync(storeUrls);
        foreach (var url in storeUrls)
        {
            ViewModel.GetListSelectors.Add(url);
        }
    }

    private async void ObjectSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Obtener el objeto Selector seleccionado
        if (ObjectSelector.SelectedItem is ListItemData selector)
        {
            _selectedCssSelector = selector.CssSelector;
            SelectorAutoSuggestBox.Text = selector.CssSelector;
            await GetAttributes();
            ElementPreviewButton_Click(sender, e);

            textBoxesStackPanel.Children.Clear();

            var listSelectors = selector.RegexMethods.ToList();//checar aqui porque se crashea

            var listPattern = listSelectors.Select(e => e.Pattern).ToList();
            var listReplacement = listSelectors.Select(e => e.Replacement).ToList();

            textBoxCount = 0;
            if (listPattern.Count == listReplacement.Count)
            {
                for (int i = 0; i < listPattern.Count; i++)
                {
                    // Agregar controles de patrón y reemplazo
                    AddNewControlsRegex(listPattern[i], listReplacement[i]);
                }
            }
            if (textBoxCount == 0)
            {
                AddNewControlsRegex(null, null);
            }

            GetAttributeComboBox.Text = selector.Attribute;
            GetTypeDataComboBox.SelectedItem = selector.Type;
            _showElementPreview = true;
        }
    }
    private async void ControlExample_Loaded(object sender, RoutedEventArgs e)
    {
        if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
        {
            var deleteCommand = new StandardUICommand(StandardUICommandKind.Delete);
            deleteCommand.ExecuteRequested += DeleteCommand_ExecuteRequested;

            await ViewModel.GetListSelectorsAsync();
            var listSelectors = ViewModel.GetListSelectors;
            foreach (var item in listSelectors)
            {
                collection.Add(new ListItemData
                {
                    CssSelector = item.CssSelector.ToString(),
                    Command = deleteCommand,
                    Id = item.Id,
                    Attribute = item.Attribute,
                    RegexMethods = item.RegexMethods,
                    Type = item.Type
                });
            }
        }
        else
        {
            await ViewModel.GetListSelectorsAsync();
            var listSelectors = ViewModel.GetListSelectors;
            foreach (var item in listSelectors)
            {
                collection.Add(new ListItemData
                {
                    CssSelector = item.CssSelector.ToString(),
                    Command = null,
                    Id = item.Id,
                    Attribute = item.Attribute,
                    RegexMethods = item.RegexMethods,
                    Type = item.Type
                });
            }
        }
    }

    private void ListViewRight_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        MenuFlyout flyout = new MenuFlyout();
        ListItemData data = (ListItemData)args.Item;
        MenuFlyoutItem item = new MenuFlyoutItem() { Command = data.Command };
        flyout.Opened += delegate (object element, object e)
        {
            MenuFlyout flyoutElement = element as MenuFlyout;
            ListViewItem elementToHighlight = flyoutElement.Target as ListViewItem;
            elementToHighlight.IsSelected = true;
        };
        flyout.Items.Add(item);
        args.ItemContainer.ContextFlyout = flyout;
    }
    private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Mouse || e.Pointer.PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Pen)
        {
            VisualStateManager.GoToState(sender as Control, "HoverButtonsShown", true);
        }
    }

    private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState(sender as Control, "HoverButtonsHidden", true);
    }

    private async void DeleteCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (args.Parameter != null)
        {
            foreach (var i in collection)
            {
                if (i.CssSelector == (args.Parameter as string))
                {
                    collection.Remove(i);
                    _ = await App.PriceTrackerService.DeleteAsync<Selector>(i.Id);
                    return;
                }
            }
        }
        if (ObjectSelector.SelectedIndex != -1)
        {
            collection.RemoveAt(ObjectSelector.SelectedIndex);
        }
    }
    private void ListView_Loaded(object sender, RoutedEventArgs e)
    {
        var listView = (ListView)sender;
        listView.ItemsSource = collection;
    }
    public async Task GetListSelectorsAsync(List<Selector> storeUrls)
    {
        collection.Clear();
        if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
        {
            var deleteCommand = new StandardUICommand(StandardUICommandKind.Delete);
            deleteCommand.ExecuteRequested += DeleteCommand_ExecuteRequested;

            foreach (var item in storeUrls)
            {
                var listItemData = new ListItemData
                {
                    Id = item.Id,
                    Attribute = item.Attribute,
                    CssSelector = item.CssSelector,
                    RegexMethods = item.RegexMethods,
                    Type = item.Type,
                    Command = deleteCommand
                };

                collection.Add(listItemData);
            }
        }
        else
        {
            foreach (var item in storeUrls)
            {
                var listItemData = new ListItemData
                {
                    Id = item.Id,
                    Attribute = item.Attribute,
                    CssSelector = item.CssSelector,
                    RegexMethods = item.RegexMethods,
                    Type = item.Type,
                    Command = null
                };

                collection.Add(listItemData);
            }
        }
    }
}