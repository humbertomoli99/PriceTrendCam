using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.Helpers;
using PriceTrendCam.ViewModels;
using Windows.Storage;

namespace PriceTrendCam.Views;

public sealed partial class OrderListContentDialog : ContentDialog
{
    public string SelectedSortBy
    {
        get; set;
    }
    public string SelectedSortDirection
    {
        get; set;
    }

    public OrderListContentDialog(string SelectedSortBy, string SelectedSortDirection)
    {
        InitializeComponent();
        this.SelectedSortBy = SelectedSortBy;
        this.SelectedSortDirection = SelectedSortDirection;
        OnApplyTemplate();
    }
    private void OnApplyTemplate()
    {
        var SortByPanel = FindName("SortByPanel") as StackPanel;
        SetCheckedRadioButtonByTagName(SortByPanel, SelectedSortBy);

        var SortDirectionPanel = FindName("SortDirectionPanel") as StackPanel;
        SetCheckedRadioButtonByTagName(SortDirectionPanel, SelectedSortDirection);
    }
    private void SetCheckedRadioButtonByTagName(StackPanel stackPanel, string tagName)
    {
        foreach (var child in stackPanel.Children)
        {
            if (child is RadioButton radioButton && radioButton.Tag.ToString() == tagName)
            {
                radioButton.IsChecked = true;
                break;
            }
        }
    }

}
