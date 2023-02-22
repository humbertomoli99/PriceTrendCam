using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using PriceTrendCam.Contracts.Services;
using PriceTrendCam.Core.Helpers;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Core.Services;

namespace PriceTrendCam.ViewModels;

public partial class AddSitemapViewModel : ObservableValidator
{
    private readonly IDialogService DialogService;

    public AddSitemapViewModel(IDialogService dialogService)
    {
        TextBoxUrls = new();
        DialogService = dialogService;

        //metodos asincronos mvvm community
        SaveSitemapCommand = new AsyncRelayCommand(SaveSitemapAsync);
        //ShowErrorsCommand = new AsyncRelayCommand(ShowErrorsAsync);
    }

    public IAsyncRelayCommand SaveSitemapCommand
    {
        get;
    }
    //public IAsyncRelayCommand ShowErrorsCommand
    //{
    //    get;
    //}
    public event EventHandler? FormSubmissionCompleted;
    public event EventHandler? FormSubmissionFailed;

    private Store? ObjectStore
    {
        get; set;
    }

    [ObservableProperty]
    private string textBoxStoreName;
    private string message;

    public List<string> TextBoxUrls
    {
        get; set;
    }

    private async Task SaveSitemapAsync()
    {
        // Validar todas las propiedades del modelo
        //ValidateAllProperties();
        message = string.Empty;

        bool anyUrlInvalid = false;
        bool anyUrlEmpty = false;

        foreach (var url in TextBoxUrls)
        {
            if (!await Url.IsValid(url))
            {
                anyUrlInvalid = true;
                break; // salimos del loop porque encontramos una URL inválida
            }
        }
        foreach (var url in TextBoxUrls)
        {
            if (url == string.Empty)
            {
                anyUrlEmpty = true;
                break; // salimos del loop porque encontramos una URL inválida
            }
        }
        if (textBoxStoreName == string.Empty || textBoxStoreName == null)
        {
            message += "The text field is required.\n";
        }
        if (anyUrlEmpty)
        {
            message += "Not all textboxes have urls\n";
        }
        else if (anyUrlInvalid)
        {
            message += "Invalid Url\n";
        }
        // Comprobar si hay errores de validación
        if (HasErrors || anyUrlInvalid)
        {
            FormSubmissionFailed?.Invoke(this, EventArgs.Empty);
            // Limpiar los valores de los TextBox
            TextBoxUrls.Clear();
            return;
        }
        else
        {
            FormSubmissionCompleted?.Invoke(this, EventArgs.Empty);
        }

        // Crear la lista de StoreUrl a partir de los valores de los TextBox
        var ListUrls = new List<StoreUrl>();

        foreach (var items in TextBoxUrls)
        {
            ListUrls.Add(new StoreUrl()
            {
                Url = items.ToString(),
            });
        }
        var FirstUrl = ListUrls.First().Url;

        var favicon = await HtmlDocumentService.GetFaviconUrlAsync(FirstUrl);

        ObjectStore = new Store
        {
            Name = textBoxStoreName,
            Favicon = favicon,
            Selectors = new List<Selector>(),
            Urls = ListUrls
        };
        // Insertar el objeto Store en la base de datos
        await App.PriceTrackerService.InsertWithChildrenAsync<Store>(ObjectStore, true);

        // Limpiar los valores de los TextBox
        ListUrls.Clear();
        TextBoxUrls.Clear();

        FormSubmissionCompleted?.Invoke(this, EventArgs.Empty);
    }
    public async Task ShowErrorsAsync(XamlRoot xamlRoot)
    {
        await DialogService.ShowMessageDialogAsync("Validation errors", message, xamlRoot);
    }
}