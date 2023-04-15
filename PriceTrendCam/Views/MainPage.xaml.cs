using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using PriceTrendCam.Helpers;
using PriceTrendCam.ViewModels;

namespace PriceTrendCam.Views;

public class ProductListItem : ObservableObject
{
    private int _id;
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    private string _title;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _imageLocation;
    public string ImageLocation
    {
        get => _imageLocation;
        set => SetProperty(ref _imageLocation, value);
    }

    private string _price;
    public string Price
    {
        get => _price;
        set => SetProperty(ref _price, value);
    }

    private string _shipping;
    public string Shipping
    {
        get => _shipping;
        set => SetProperty(ref _shipping, value);
    }

    private string _stock;
    public string Stock
    {
        get => _stock;
        set => SetProperty(ref _stock, value);
    }
}

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get; set;
    }

    public MainPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<MainViewModel>();
        Loaded += MainPage_Loaded;
    }

    private async void MainPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        try
        {
            ViewModel.ListViewProducts = ListViewProducts;
            ViewModel.xamlRoot = XamlRoot;

            // Cargar los productos en la lista
            await ViewModel.UpdateListCommand();

            // Establecer el contexto de datos del ListView
            DataContext = ViewModel;
        }
        catch (Exception ex)
        {
            await AppCenterHelper.ShowErrorDialog(ex, XamlRoot);
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
            await AppCenterHelper.ShowErrorDialog(ex, XamlRoot);
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
            await AppCenterHelper.ShowErrorDialog(ex, XamlRoot);
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
            await AppCenterHelper.ShowErrorDialog(ex, XamlRoot);
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
            await AppCenterHelper.ShowErrorDialog(ex, XamlRoot);
        }
    }
}
