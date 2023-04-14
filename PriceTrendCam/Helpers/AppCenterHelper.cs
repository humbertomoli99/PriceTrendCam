using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AppCenter.Crashes;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PriceTrendCam.Helpers;

public static class AppCenterHelper
{
    public static async Task ShowErrorDialog(Exception ex, XamlRoot xamlRoot)
    {
        ContentDialog dialog = new ContentDialog
        {
            XamlRoot = xamlRoot,
            Title = "Ocurrió un error",
            Content = "Se ha producido un error. ¿Desea enviar información del error a App Center para ayudarnos a solucionarlo?",
            PrimaryButtonText = "Enviar",
            SecondaryButtonText = "No enviar",
            CloseButtonText = "Cancelar"
        };

        ContentDialogResult result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ContentDialog alwaysSendDialog = new ContentDialog
            {
                Title = "Enviar siempre",
                Content = "¿Desea enviar automáticamente información del error a App Center en el futuro?",
                PrimaryButtonText = "Si",
                SecondaryButtonText = "No"
            };
            ContentDialogResult alwaysSendResult = await alwaysSendDialog.ShowAsync();
            switch (alwaysSendResult)
            {
                case ContentDialogResult.Primary:
                    Crashes.NotifyUserConfirmation(UserConfirmation.AlwaysSend);
                    break;
                case ContentDialogResult.Secondary:
                    Crashes.NotifyUserConfirmation(UserConfirmation.Send);
                    break;
            }
        }
        else if (result == ContentDialogResult.Secondary)
        {
            Crashes.NotifyUserConfirmation(UserConfirmation.DontSend);
        }
    }
}

