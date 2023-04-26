using Microsoft.UI.Xaml;
using System.Text;
using Microsoft.UI.Xaml.Controls;

using PriceTrendCam.ViewModels;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Collections.ObjectModel;

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

    public NewWebScrapingToolsProfilePage()
    {
        ViewModel = App.GetService<NewWebScrapingToolsProfileViewModel>();
        InitializeComponent();
        InitializeWebAutomationToolsComboBox();
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
            stackHtmlAgilityPackConfig.Visibility = Visibility.Visible;
        }
        else
        {
            stackHtmlAgilityPackConfig.Visibility = Visibility.Collapsed;
        }
    }
    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        // Aquí puedes guardar los datos del formulario según tus necesidades
        string profileName = tbProfileName.Text;
        string webAutomationTool = cmbWebAutomationTool.SelectedItem as string;
        string encoding = tbEncoding.Text;
        string userAgent = tbUserAgent.Text;
        string cookie = tbCookie.Text;
        string decompressionMethod = cmbDecompressionMethod.SelectedItem as string;

        // Realiza la lógica de guardado de datos aquí
    }
}
