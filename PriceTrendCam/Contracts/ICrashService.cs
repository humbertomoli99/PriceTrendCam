using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AppCenter.Crashes;

namespace PriceTrendCam.Contracts;
public interface ICrashService
{
    Task InitializeAsync();
    Task SetUserConfirmationAsync(UserConfirmation value);
    Task<UserConfirmation> LoadUserConfirmationFromSettingsAsync();
    Task SaveUserConfirmationInSettingsAsync(UserConfirmation value);
}