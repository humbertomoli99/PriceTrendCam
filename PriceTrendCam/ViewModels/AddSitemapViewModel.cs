using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using PriceTrendCam.Core.Models;
using Windows.Storage;

namespace PriceTrendCam.ViewModels;

public class AddSitemapViewModel : ObservableRecipient
{
    private TextBox _StoreName;
    private TextBox _StoreURL;
    public AddSitemapViewModel(object[] campos)
    {
        _StoreName = (TextBox)campos[0];
        _StoreURL = (TextBox)campos[1];
    }
    public AddSitemapViewModel()
    {
    }
    public ICommand ICommandAddStore => new RelayCommand(new Action(async () => await AddStoreAsync()));

    public async Task AddStoreAsync()
    {
    }
}
