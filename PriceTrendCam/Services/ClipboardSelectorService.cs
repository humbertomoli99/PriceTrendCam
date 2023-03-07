using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PriceTrendCam.Contracts.Services;

namespace PriceTrendCam.Services;
public class ClipboardSelectorService : IClipboardSelectorService
{
    private const string SettingsKey = "PasteLinksFromClipboardAutomatically";

    public bool AutomaticallyPasteLinksFromClipboard { get; set; } = false;

    private readonly ILocalSettingsService _localSettingsService;

    public ClipboardSelectorService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    public async Task InitializeAsync()
    {
        AutomaticallyPasteLinksFromClipboard = await LoadClipboardSettingFromSettingsAsync();
        await Task.CompletedTask;
    }

    public async Task SetClipboardSettingAsync(bool value)
    {
        AutomaticallyPasteLinksFromClipboard = value;

        await SaveClipboardSettingInSettingsAsync(AutomaticallyPasteLinksFromClipboard);
    }

    public async Task<bool> LoadClipboardSettingFromSettingsAsync()
    {
        return await _localSettingsService.ReadSettingAsync<bool>(SettingsKey);
    }

    public async Task SaveClipboardSettingInSettingsAsync(bool value)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKey, value);
    }
}
