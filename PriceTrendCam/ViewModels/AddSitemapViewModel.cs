using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.Contracts.Services;
using PriceTrendCam.Core.Helpers;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Core.Services;
using PriceTrendCam.Views;

namespace PriceTrendCam.ViewModels;

public partial class AddSitemapViewModel : ObservableValidator
{
    private readonly IDialogService DialogService;

    public AddSitemapViewModel(IDialogService dialogService)
    {
        TextBoxUrl = new();
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
    [Required(ErrorMessage = "El campo de texto es obligatorio.")]
    private string textBoxStoreName;
    public List<string> TextBoxUrl
    {
        get; set;
    }

    private async Task SaveSitemapAsync()
    {
        // Validar todas las propiedades del modelo, incluyendo los nuevos TextBox
        //ValidateProperty(nameof(textBoxStoreName));
        //ValidateProperty(nameof(TextBoxUrl));
        ValidateAllProperties();

        // Comprobar si hay errores de validación
        if (HasErrors)
        {
            FormSubmissionFailed?.Invoke(this, EventArgs.Empty);
            return;
        }
        else
        {
            FormSubmissionCompleted?.Invoke(this, EventArgs.Empty);
        }

        // Crear la lista de StoreUrl a partir de los valores de los TextBox
        var textBoxUrls = new List<StoreUrl>();

        foreach (var items in TextBoxUrl)
        {
            textBoxUrls.Add(new StoreUrl()
            {
                Url = items.ToString(),
            });
        }
        var FirstUrl = textBoxUrls.First().Url;

        if(await Url.IsValid(FirstUrl) == false)
        {
            Debug.WriteLine("the url is invalid");
            return;
        }
        //var node = await HtmlDocumentService.LoadPageAsync(FirstUrl);

        var favicon = await HtmlDocumentService.GetFaviconUrlAsync(FirstUrl);

        ObjectStore = new Store
        {
            Name = textBoxStoreName,
            Favicon = favicon,
            Selectors = new List<Selector>(),
            Urls = textBoxUrls
        };
        // Insertar el objeto Store en la base de datos
        await App.PriceTrackerService.InsertWithChildrenAsync<Store>(ObjectStore, true);

        // Limpiar los valores de los TextBox
        textBoxStoreName = null;
        textBoxUrls.Clear();

        FormSubmissionCompleted?.Invoke(this, EventArgs.Empty);
    }
    public async Task ShowErrorsAsync(XamlRoot xamlRoot)
    {
        var message = string.Join(Environment.NewLine, GetErrors().Select(e => e.ErrorMessage));

        await DialogService.ShowMessageDialogAsync("Validation errors", message, xamlRoot);
    }
}