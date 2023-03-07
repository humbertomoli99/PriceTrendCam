using Microsoft.UI.Xaml.Controls;

using PriceTrendCam.ViewModels;
using Windows.System;

namespace PriceTrendCam.Views;

// TODO: Set the URL for your privacy policy by updating SettingsPage_PrivacyTermsLink.NavigateUri in Resources.resw.
public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
    }

    private void HyperlinkButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Uri webPage = new Uri("ms-settings:notifications");
        _ = Launcher.LaunchUriAsync(webPage);
    }
}
