using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using PriceTrendCam.Core.Helpers;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Core.Services;
using PriceTrendCam.ViewModels;

namespace PriceTrendCam.Views;

public sealed partial class AddSitemapPage : Page
{
    private AddSitemapViewModel ViewModel => (AddSitemapViewModel)DataContext;

    public AddSitemapPage()
    {
        InitializeComponent();
        DataContext = App.GetService<AddSitemapViewModel>();
        ViewModel.TextBoxUrls = new();
        Agregar_Click1(null, null);
    }

    private int textBoxCount = 0;
    private void Eliminar_Click1(object sender, RoutedEventArgs e)
    {
        Button? deleteButton = (Button)sender;
        Grid? grid = (Grid)deleteButton.Tag;
        textBoxesStackPanel.Children.Remove(grid);

        textBoxCount--;

        if (textBoxCount == 1)
        {
            foreach (UIElement element in textBoxesStackPanel.Children)
            {
                if (element is Grid)
                {
                    Grid g = (Grid)element;
                    Button button = (Button)g.Children[1];
                    button.IsEnabled = false;
                    break;
                }
            }
        }
    }
    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        List<string> textBoxValues = new List<string>();

        foreach (UIElement element in textBoxesStackPanel.Children)
        {
            if (element is Grid)
            {
                Grid grid = (Grid)element;
                TextBox textBox = (TextBox)grid.Children[0];
                textBoxValues.Add(textBox.Text);
            }
        }
        List<StoreUrl> textBoxUrls = new List<StoreUrl>();
        foreach (var items in textBoxValues)
        {
            textBoxUrls.Add(new StoreUrl()
            {
                Url = items.ToString(),
            });
        }
        foreach (var items in textBoxValues)
        {
            ViewModel.TextBoxUrls?.Add(items);
        }
    }
    private void Agregar_Click1(object? sender, RoutedEventArgs? e)
    {
        // Crear un nuevo grid para cada TextBox y botón
        Grid newGrid = new Grid();

        // Columna para el TextBox
        ColumnDefinition column1 = new ColumnDefinition();
        column1.Width = new GridLength(1, GridUnitType.Star);
        newGrid.ColumnDefinitions.Add(column1);

        // Columna para el botón
        ColumnDefinition column2 = new ColumnDefinition();
        column2.Width = GridLength.Auto;
        newGrid.ColumnDefinitions.Add(column2);

        // Crear un nuevo TextBox
        TextBox newTextBox = new TextBox();
        newTextBox.Margin = new Thickness(0, 6, 0, 0);
        newTextBox.LostFocus += NewTextBox_LostFocus;
        newTextBox.IsTextPredictionEnabled = false;

        // Agregar el TextBox a la primera columna del nuevo Grid
        Grid.SetColumn(newTextBox, 0);
        newGrid.Children.Add(newTextBox);

        // Crear un nuevo botón
        Button deleteButton = new Button();
        deleteButton.Margin = new Thickness(6, 6, 0, 0);

        deleteButton.Content = "🗑";
        deleteButton.Tag = newGrid;
        deleteButton.Click += Eliminar_Click1;

        // Agregar el botón a la segunda columna del nuevo Grid
        Grid.SetColumn(deleteButton, 1);
        newGrid.Children.Add(deleteButton);

        // Agregar el nuevo Grid al StackPanel
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
            addButton.Click += Agregar_Click1;

            Grid.SetColumn(addButton, 2);
            newGrid.Children.Add(addButton);

            newTextBox.PlaceholderText = "https://";
            newTextBox.Header = "Start URL";
            newTextBox.Margin = new Thickness(0, 0, 0, 0);
            newTextBox.TextChanged += StoreUrlTextBox_TextChanged;
            newTextBox.LostFocus += NewTextBox_LostFocus;
            newTextBox.IsTextPredictionEnabled = false;
        }
    }

    private async void NewTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        textBoxesStackPanel.Children
            .OfType<Grid>()
            .SelectMany(grid => grid.Children.OfType<TextBox>())
            .ToList()
            .ForEach(async textBox => textBox.Text = await Url.NormalizeUrl(textBox.Text));
    }

    private async void StoreUrlTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            Grid firstGrid = textBoxesStackPanel.Children[0] as Grid;
            TextBox firstTextBox = firstGrid.Children[0] as TextBox;

            // Utiliza el primer TextBox aquí
            string url = firstTextBox.Text;
            string faviconUrlString = await HtmlDocumentService.GetFaviconUrlAsync(url);
            BitmapImage faviconImage = new BitmapImage(new Uri(faviconUrlString));
            FaviconUrl.Source = faviconImage;
        }
        catch
        {
            //Aquí puedes manejar las excepciones si ocurren
        }
    }
    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ShowErrorsAsync(this.XamlRoot);
    }
}
