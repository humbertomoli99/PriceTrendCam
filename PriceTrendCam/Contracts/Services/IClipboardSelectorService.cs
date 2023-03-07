using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceTrendCam.Contracts.Services;
public interface IClipboardSelectorService
{
    bool AutomaticallyPasteLinksFromClipboard
    {
        get; set;
    }

    Task<bool> LoadClipboardSettingFromSettingsAsync();

    Task SaveClipboardSettingInSettingsAsync(bool value);
}