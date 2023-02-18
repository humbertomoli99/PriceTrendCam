using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Services;
using PriceTrendCam.ViewModels;

namespace PriceTrendCam.Views;

public sealed partial class AddSitemapPage : Page
{
    private AddSitemapViewModel ViewModel => (AddSitemapViewModel)DataContext;
    public AddSitemapPage()
    {
        InitializeComponent();
        DataContext = App.GetService<AddSitemapViewModel>();
        ViewModel.TextBoxUrl = new();
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
        foreach(var items in textBoxValues)
        {
            ViewModel.TextBoxUrl?.Add(items);
        }
        _ = ViewModel.SaveCommand;
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
            addButton.Margin = new Thickness(6, 0, 0, 0);

            addButton.Content = "➕";
            addButton.Tag = newGrid;
            addButton.Click += Agregar_Click1;

            Grid.SetColumn(addButton, 2);
            newGrid.Children.Add(addButton);

            newTextBox.PlaceholderText = "Url";
            newTextBox.Margin = new Thickness(0, 0, 0, 0);
        }
    }
}
