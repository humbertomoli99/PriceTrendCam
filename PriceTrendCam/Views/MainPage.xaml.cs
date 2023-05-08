using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using PriceTrendCam.Helpers;
using PriceTrendCam.Models;
using PriceTrendCam.ViewModels;

namespace PriceTrendCam.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get; set;
    }

    public ContentDialogHelper<ContentDialog> ContentDialogHelper
    {
        get; set;
    }
    public MainPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<MainViewModel>();
        ContentDialogHelper = ContentDialogHelper<ContentDialog>.Instance;
        Loaded += MainPage_Loaded;
    }

    private async void MainPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        try
        {
            ViewModel.ListViewProducts = ListViewProducts;
            ViewModel.XamlRoot = XamlRoot;

            // Cargar los productos en la lista
            //await ViewModel.UpdateList();

            // Establecer el contexto de datos del ListView
            DataContext = ViewModel;
        }
        catch (Exception ex)
        {
            await ContentDialogHelper.ShowExceptionDialog(ex, XamlRoot);
        }
    }

    private async void Page_GotFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        try
        {
            await ViewModel.ShowMessageAddProductFromClipboard();

        }
        catch (Exception ex)
        {
            await ContentDialogHelper.ShowExceptionDialog(ex, XamlRoot);
        }
    }
    private async void CheckBox_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        try
        {
            if (ListViewProducts.SelectionMode == ListViewSelectionMode.Multiple || ListViewProducts.SelectionMode == ListViewSelectionMode.Extended)
            {
                SelectAllCheckBox.IsChecked = true;
                SelectAllCheckBoxIcon.Glyph = "\ue73a";
                ListViewProducts.SelectAll();
            }
        }
        catch (Exception ex)
        {
            await ContentDialogHelper.ShowExceptionDialog(ex, XamlRoot);
        }
    }

    private async void CheckBox_Unchecked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        try
        {
            if (ListViewProducts.SelectionMode == ListViewSelectionMode.Multiple || ListViewProducts.SelectionMode == ListViewSelectionMode.Extended)
            {
                SelectAllCheckBox.IsChecked = false;
                SelectAllCheckBoxIcon.Glyph = "\ue739";
                ListViewProducts.DeselectRange(new ItemIndexRange(0, (uint)ListViewProducts.Items.Count));
            }
        }
        catch (Exception ex)
        {
            await ContentDialogHelper.ShowExceptionDialog(ex, XamlRoot);
        }
    }

    private async void ListViewProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            ViewModel.SelectAllCheckBox = SelectAllCheckBox;
            ViewModel.SelectAllCheckBoxIcon = SelectAllCheckBoxIcon;
            await ViewModel.HandleSelectionChangedAsync(e.AddedItems);
        }
        catch (Exception ex)
        {
            await ContentDialogHelper.ShowExceptionDialog(ex, XamlRoot);
        }
    }
    private void ListViewRight_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        MenuFlyout flyout = new MenuFlyout();
        ProductListItem data = (ProductListItem)args.Item;
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

    private async void RowsPerPageOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            if (RowsPerPageOptions.SelectedItem != null && int.TryParse(RowsPerPageOptions.SelectedItem as string, out int selectedValue))
            {
                viewModel.SelectedRowsPerPageOption = selectedValue;
                await ViewModel.OnSelectedItemChanged();
            }
        }
    }
}
