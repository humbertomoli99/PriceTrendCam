using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AppCenter.Crashes;
using PriceTrendCam.Contracts;
using PriceTrendCam.Contracts.Services;

namespace PriceTrendCam.Services;
public class CrashService : ICrashService
{
    private const string UserConfirmationKey = "CrashUserConfirmation";

    private readonly ILocalSettingsService _localSettingsService;

    public CrashService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    public async Task InitializeAsync()
    {
        Crashes.NotifyUserConfirmation(await LoadUserConfirmationFromSettingsAsync());
        await Task.CompletedTask;
    }

    public async Task SetUserConfirmationAsync(UserConfirmation value)
    {
        await SaveUserConfirmationInSettingsAsync(value);
        Crashes.NotifyUserConfirmation(value);
    }

    public async Task<UserConfirmation> LoadUserConfirmationFromSettingsAsync()
    {
        var confirmation = await _localSettingsService.ReadSettingAsync<int>(UserConfirmationKey);
        return (UserConfirmation)confirmation;
    }

    public async Task SaveUserConfirmationInSettingsAsync(UserConfirmation value)
    {
        await _localSettingsService.SaveSettingAsync(UserConfirmationKey, (int)value);
    }
}
