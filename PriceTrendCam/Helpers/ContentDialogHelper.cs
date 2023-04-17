using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.AppCenter.Crashes;

namespace PriceTrendCam.Helpers;
public class ContentDialogHelper<T>
{
    private static ContentDialogHelper<T> _instance;
    private static ContentDialog _currentDialog;
    private static readonly object _lock = new object();

    private ContentDialogHelper()
    {
    }

    public static ContentDialogHelper<T> Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new ContentDialogHelper<T>();
                }
                return _instance;
            }
        }
    }

    public async Task<ContentDialogResult> ShowContentDialog(ContentDialog dialog, XamlRoot xamlRoot)
    {
        if (_currentDialog != null)
        {
            // A dialog is already open, so we don't need to show another one
            return ContentDialogResult.None;
        }

        dialog.XamlRoot = xamlRoot;
        _currentDialog = dialog;

        ContentDialogResult result = await _currentDialog.ShowAsync();
        _currentDialog = null;

        return result;
    }
    public static async Task ShowErrorDialog(Exception ex, XamlRoot xamlRoot)
    {
        ContentDialog dialog = new ContentDialog
        {
            Title = "Ocurrió un error",
            Content = "Se ha producido un error. ¿Desea enviar información del error a App Center para ayudarnos a solucionarlo?",
            PrimaryButtonText = "Enviar",
            SecondaryButtonText = "No enviar",
            CloseButtonText = "Cancelar"
        };

        ContentDialogHelper<ContentDialog> dialogHelper = ContentDialogHelper<ContentDialog>.Instance;
        ContentDialogResult result = await dialogHelper.ShowContentDialog(dialog, xamlRoot);

        if (result == ContentDialogResult.Primary)
        {
            ContentDialog alwaysSendDialog = new ContentDialog
            {
                Title = "Enviar siempre",
                Content = "¿Desea enviar automáticamente información del error a App Center en el futuro?",
                PrimaryButtonText = "Si",
                SecondaryButtonText = "No"
            };

            ContentDialogResult alwaysSendResult = await dialogHelper.ShowContentDialog(alwaysSendDialog, xamlRoot);
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