using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

namespace PriceTrendCam.ViewModels;

public partial class NotificationSettingsContentDialogViewModel : ObservableObject
{
    [ObservableProperty]
    public bool isNotificationsAlwaysSelected;

    [ObservableProperty]
    public bool isNotificationsAboveSelected;

    [ObservableProperty]
    public bool isNotificationsBelowSelected;

    [ObservableProperty]
    public Visibility textBoxNotificationPreferenceVisibility;

    public NotificationSettingsContentDialogViewModel()
    {
        SetTextBoxVisibility(false);
        isNotificationsAlwaysSelected = true;
    }

    public void SetTextBoxVisibility(bool isVisible)
    {
        TextBoxNotificationPreferenceVisibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    [RelayCommand]
    public void AlwaysCommand() => SetTextBoxVisibility(false);

    [RelayCommand]
    public void AboveCommand() => SetTextBoxVisibility(true);

    [RelayCommand]
    public void BelowCommand() => SetTextBoxVisibility(true);

}
