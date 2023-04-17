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

    public async Task<ContentDialogResult> ShowContentDialog(T dialog)
    {
        if (isDialogOpen)
        {
            return ContentDialogResult.None;
        }

        isDialogOpen = true;
        ContentDialogResult result;
        try
        {
            result = await dialog.ShowAsync();
        }
        catch (Exception)
        {
            dialog.Hide();
            result = ContentDialogResult.None;
        }
        isDialogOpen = false;

        return result;
    }

    public async Task ShowExceptionDialog(Exception ex, XamlRoot xamlRoot)
    {
        T dialog = Activator.CreateInstance<T>();
        dialog.Title = "Ocurrió un error";
        dialog.Content = "Se ha producido un error. ¿Desea enviar información del error a App Center para ayudarnos a solucionarlo?";
        dialog.PrimaryButtonText = "Enviar";
        dialog.SecondaryButtonText = "No enviar";
        dialog.CloseButtonText = "Cancelar";
        dialog.XamlRoot = xamlRoot;

        ContentDialogResult result = await ShowContentDialog(dialog);

        if (result == ContentDialogResult.Primary)
        {
            T alwaysSendDialog = Activator.CreateInstance<T>();
            alwaysSendDialog.Title = "Enviar siempre";
            alwaysSendDialog.Content = "¿Desea enviar automáticamente información del error a App Center en el futuro?";
            alwaysSendDialog.PrimaryButtonText = "Si";
            alwaysSendDialog.SecondaryButtonText = "No";
            alwaysSendDialog.XamlRoot = xamlRoot;

            ContentDialogResult alwaysSendResult = await ShowContentDialog(alwaysSendDialog);
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