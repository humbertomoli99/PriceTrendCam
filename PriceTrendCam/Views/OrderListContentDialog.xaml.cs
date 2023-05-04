using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.Helpers;
using PriceTrendCam.ViewModels;
using Windows.Storage;

namespace PriceTrendCam.Views;

// Declaración de la clase OrderListContentDialog, que hereda de ContentDialog y es sealed (no puede ser heredada por otras clases)
public sealed partial class OrderListContentDialog : ContentDialog
{
    // Propiedad pública SelectedSortBy
    private string SelectedSortBy
    {
        get; set;
    }

    // Propiedad pública SelectedSortDirection
    private string SelectedSortDirection
    {
        get; set;
    }

    // Constructor de la clase OrderListContentDialog
    public OrderListContentDialog(string SelectedSortBy, string SelectedSortDirection)
    {
        // Llamada al constructor de la clase base (ContentDialog)
        InitializeComponent();

        // Asignar los valores de los parámetros a las propiedades SelectedSortBy y SelectedSortDirection
        this.SelectedSortBy = SelectedSortBy;
        this.SelectedSortDirection = SelectedSortDirection;

        // Configurar los radio buttons seleccionados según los valores de SelectedSortBy y SelectedSortDirection
        SetCheckedRadioButtonByTagNameForStackPanel();
    }

    // Método privado para configurar los radio buttons seleccionados en un StackPanel basado en un nombre de etiqueta
    private void SetCheckedRadioButtonByTagNameForStackPanel()
    {
        // Encontrar el StackPanel con el nombre "SortByPanel" y asignarlo a la variable SortByPanel
        var SortByPanel = FindName("SortByPanel") as StackPanel;

        // Configurar el radio button seleccionado en el SortByPanel basado en el valor de SelectedSortBy
        SetCheckedRadioButtonByTagName(SortByPanel, SelectedSortBy);

        // Encontrar el StackPanel con el nombre "SortDirectionPanel" y asignarlo a la variable SortDirectionPanel
        var SortDirectionPanel = FindName("SortDirectionPanel") as StackPanel;

        // Configurar el radio button seleccionado en el SortDirectionPanel basado en el valor de SelectedSortDirection
        SetCheckedRadioButtonByTagName(SortDirectionPanel, SelectedSortDirection);
    }

    // Método privado para configurar un radio button seleccionado en un StackPanel basado en un nombre de etiqueta
    private void SetCheckedRadioButtonByTagName(StackPanel stackPanel, string tagName)
    {
        // Iterar sobre cada elemento hijo del StackPanel
        foreach (var child in stackPanel.Children)
        {
            // Verificar si el elemento es un RadioButton y si su etiqueta (Tag) coincide con tagName
            if (child is RadioButton radioButton && radioButton.Tag.ToString() == tagName)
            {
                // Marcar el radio button como seleccionado
                radioButton.IsChecked = true;
                // Salir del bucle, ya que solo queremos seleccionar un radio button
                break;
            }
        }
    }
}