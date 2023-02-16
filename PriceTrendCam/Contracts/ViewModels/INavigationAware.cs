namespace PriceTrendCam.Contracts.ViewModels;

public interface INavigationAware
{
    Task OnNavigatedTo(object parameter);

    void OnNavigatedFrom();
}
