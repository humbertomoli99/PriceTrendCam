using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.AppCenter.Crashes;

namespace PriceTrendCam.Helpers;
public class ContentDialogHelper<T> where T : ContentDialog
{
    private static ContentDialogHelper<T> instance;

    public static ContentDialogHelper<T> Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new ContentDialogHelper<T>();
            }
            return instance;
        }
    }

    private bool isDialogOpen = false;

    public async Task<ContentDialogResult> ShowContentDialog(T dialog, XamlRoot xamlRoot)
    {
        if (isDialogOpen)
        {
            return ContentDialogResult.None;
        }

        isDialogOpen = true;
        dialog.XamlRoot = xamlRoot;
        ContentDialogResult result = await dialog.ShowAsync();
        isDialogOpen = false;

        return result;
    }

    public async Task ShowErrorDialog(Exception ex, XamlRoot xamlRoot)
    {
        T dialog = Activator.CreateInstance<T>();
        dialog.Title = "Ocurrió un error";
        dialog.Content = "Se ha producido un error. ¿Desea enviar información del error a App Center para ayudarnos a solucionarlo?";
        dialog.PrimaryButtonText = "Enviar";
        dialog.SecondaryButtonText = "No enviar";
        dialog.CloseButtonText = "Cancelar";

        ContentDialogResult result = await ShowContentDialog(dialog, xamlRoot);

        if (result == ContentDialogResult.Primary)
        {
            T alwaysSendDialog = Activator.CreateInstance<T>();
            alwaysSendDialog.Title = "Enviar siempre";
            alwaysSendDialog.Content = "¿Desea enviar automáticamente información del error a App Center en el futuro?";
            alwaysSendDialog.PrimaryButtonText = "Si";
            alwaysSendDialog.SecondaryButtonText = "No";

            ContentDialogResult alwaysSendResult = await ShowContentDialog(alwaysSendDialog, xamlRoot);
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