using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.Core.Contracts.Services;

namespace PriceTrendCam.Services;
/// <summary>
/// A <see langword="class"/> that implements the <see cref="IDialogService"/> <see langword="interface"/> using UWP APIs.
/// </summary>
public sealed class DialogService : IDialogService
{
    /// <inheritdoc/>
    public Task ShowMessageDialogAsync(string title, string message)
    {
        ContentDialog dialog = new();
        dialog.Title = title;
        dialog.CloseButtonText = "Close";
        dialog.DefaultButton = ContentDialogButton.Close;
        dialog.Content = message;

        return dialog.ShowAsync().AsTask();
    }
}