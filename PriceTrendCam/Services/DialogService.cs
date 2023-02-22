using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.Contracts.Services;
using PriceTrendCam.Views;

namespace PriceTrendCam.Services;
/// <summary>
/// A <see langword="class"/> that implements the <see cref="IDialogService"/> <see langword="interface"/> using UWP APIs.
/// </summary>
public sealed class DialogService : IDialogService
{
    /// <inheritdoc/>
    public Task ShowMessageDialogAsync(string title, string message,XamlRoot xamlRoot)
    {

        ContentDialog dialog = new()
        {
            Title = title,
            XamlRoot = xamlRoot.Content.XamlRoot,
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Close,
            Content = message
        };

        return dialog.ShowAsync().AsTask();
    }
}