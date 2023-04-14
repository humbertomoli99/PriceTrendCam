using CommunityToolkit.Mvvm.ComponentModel;
using PriceTrendCam.Contracts.ViewModels;

namespace PriceTrendCam.ViewModels;

public class ProductsListViewModel : ObservableRecipient, INavigationAware
{
    public ProductsListViewModel()
    {
    }

    public void OnNavigatedFrom()
    {

    }
    public Task OnNavigatedTo(object parameter)
    {
        return Task.CompletedTask;
    }
}
