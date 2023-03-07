using System.Reflection;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.Contracts.Services;
using PriceTrendCam.Helpers;
using PriceTrendCam.Services;
using Windows.ApplicationModel;

namespace PriceTrendCam.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
    private ElementTheme _elementTheme;
    private string _versionDescription;
    private readonly IClipboardSelectorService _clipboardSelectorService;

    public ElementTheme ElementTheme
    {
        get => _elementTheme;
        set => SetProperty(ref _elementTheme, value);
    }

    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }

    public ICommand SwitchThemeCommand
    {
        get;
    }

    public bool ClipboardAutomatically
    {
        get;set;
    }
    
    public SettingsViewModel(IThemeSelectorService themeSelectorService, IClipboardSelectorService clipboardSelectorService)
    {
        _clipboardSelectorService = clipboardSelectorService;

        _themeSelectorService = themeSelectorService;

        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();

        _ = GetBoolClipboard();

        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);
                }
            });
    }

    private async Task GetBoolClipboard()
    {
        ClipboardAutomatically = await _clipboardSelectorService.LoadClipboardSettingFromSettingsAsync();
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
    public async void OnToggleSwitchToggled(object sender, RoutedEventArgs e)
    {
        // Se cambia el valor de ClipboardAutomatically según si está activado o no el ToggleSwitch
        ClipboardAutomatically = ((ToggleSwitch)sender).IsOn;

        // Se guarda el valor en localSettings
        await _clipboardSelectorService.SaveClipboardSettingInSettingsAsync(ClipboardAutomatically).ConfigureAwait(false);
    }
}
