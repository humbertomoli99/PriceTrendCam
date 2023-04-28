using Microsoft.UI.Xaml;
using System.Text;
using Microsoft.UI.Xaml.Controls;

using PriceTrendCam.ViewModels;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Collections.ObjectModel;
using System.Net;

namespace PriceTrendCam.Views;

public sealed partial class NewWebScrapingToolsProfilePage : Page
{
    public NewWebScrapingToolsProfileViewModel ViewModel
    {
        get;
    }
    public ObservableCollection<string> SelectedWebAutomationTools
    {
        get;
        set;
    }
    public ObservableCollection<string> SelectedWebBrowsers
    {
        get;
        set;
    }
    public ObservableCollection<string> DecompresionMethodComboBox
    {
        get;
        set;
    }

    public NewWebScrapingToolsProfilePage()
    {
        ViewModel = App.GetService<NewWebScrapingToolsProfileViewModel>();
        InitializeComponent();
        InitializeWebAutomationToolsComboBox();
        InitializeWebBrowserComboBox();
        InitializeDecompresionMethodComboBox();
    }
    private void InitializeDecompresionMethodComboBox()
    {
        DecompresionMethodComboBox = new ObservableCollection<string>(Enum.GetNames(typeof(DecompressionMethods)));
    }
    private void InitializeWebBrowserComboBox()
    {
        SelectedWebBrowsers = new ObservableCollection<string>(Enum.GetNames(typeof(WebBrowsers)));
    }
    private void InitializeWebAutomationToolsComboBox()
    {
        SelectedWebAutomationTools = new ObservableCollection<string>(Enum.GetNames(typeof(WebScrapingTools)));
    }
    private void cmbWebAutomationTool_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedItem = cmbWebAutomationTool.SelectedItem;
        if (selectedItem.ToString() == "HtmlAgilityPack")
        {
            spHttpClientHandler.Visibility = Visibility.Visible;
            spHttpClient.Visibility = Visibility.Visible;

            stackWebBrowsers.Visibility = Visibility.Collapsed;
        }
        else if(selectedItem.ToString() == "SeleniumWebDriver")
        {
            spHttpClientHandler.Visibility = Visibility.Collapsed;
            spHttpClient.Visibility = Visibility.Collapsed;

            stackWebBrowsers.Visibility = Visibility.Visible;
        }
        else
        {
            spHttpClientHandler.Visibility = Visibility.Collapsed;
            spHttpClient.Visibility = Visibility.Collapsed;

            stackWebBrowsers.Visibility = Visibility.Collapsed;
        }
    }
    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        // Aquí puedes guardar los datos del formulario según tus necesidades
        string profileName = tbProfileName.Text;
        string webAutomationTool = cmbWebAutomationTool.SelectedItem as string;
        string userAgent = tbUserAgent.Text;
        string cookie = tbCookie.Text;
        string decompressionMethod = cmbDecompressionMethod.SelectedItem as string;

        // Realiza la lógica de guardado de datos aquí
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {

    }
}
