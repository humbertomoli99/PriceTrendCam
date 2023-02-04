using Microsoft.UI.Xaml.Controls;

using PriceTrendCam.ViewModels;

namespace PriceTrendCam.Views;

// TODO: Change the grid as appropriate for your app. Adjust the column definitions on DataGridPage.xaml.
// For more details, see the documentation at https://docs.microsoft.com/windows/communitytoolkit/controls/datagrid.
public sealed partial class ExportDataPage : Page
{
    public ExportDataViewModel ViewModel
    {
        get;
    }

    public ExportDataPage()
    {
        ViewModel = App.GetService<ExportDataViewModel>();
        InitializeComponent();
    }
}
