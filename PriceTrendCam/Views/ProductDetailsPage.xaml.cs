using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.Helpers;
using PriceTrendCam.ViewModels;

namespace PriceTrendCam.Views;

public sealed partial class ProductDetailsPage : Page
{
    public ProductDetailsViewModel ViewModel
    {
        get;
    }
    public ContentDialogHelper<ContentDialog> ContentDialogHelper
    {
        get; set;
    }
    public ProductDetailsPage()
    {
        ViewModel = App.GetService<ProductDetailsViewModel>();
        InitializeComponent();
        Loaded += ProductDetailsPage_Loaded;
    }

    private void ProductDetailsPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Establecer el contexto de datos del ListView
        DataContext = ViewModel;
    }

    private void flip_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        flip.Height = flip.ActualWidth * 0.5625;
    }

    private async void BtnEditNotification_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var dialogNotifications = new NotificationSettingsContentDialog()
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
        };

        ContentDialogResult dialogResult = await ContentDialogHelper<ContentDialog>.Instance.ShowContentDialog(dialogNotifications);

        if (dialogResult != ContentDialogResult.Primary)
            return;
    }
}
